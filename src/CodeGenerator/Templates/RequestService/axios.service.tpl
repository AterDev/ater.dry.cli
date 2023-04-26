import axios from 'axios'
import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  Method
} from 'axios'
import cancelPendingRequest from '@/_packages/request/cancelPendingRequest'
import cacheTaskResponse from '@/_packages/request/cacheTaskResponse'
import loadingService from '@/_packages/request/loading'
import { useSingleMsg } from '@/hooks/utils'
import router from '@/router'
import { storage } from '@/utils'
import mockUrls from '@/_config/mock.json'

declare module 'axios' {
  interface AxiosRequestConfig {
    /**
     * 开启loading(默认开启)
     */
    loading?: boolean
    /**
     * 最大重试次数(默认3次)
     */
    retryTimes?: number
    /**
     * 重试延迟(默认100毫秒)
     */
    retryDelay?: number
    /**
     * 当前重试次数
     */
    retryCount?: number
    /**
     * 缓存, 只取最后一次返回结果(默认关闭)
     */
    cache_task?: boolean
    cache_task_config?: any
    /**
     * 取消重复请求(默认关闭)
     */
    repeat_cancel?: boolean
    /**
     * 开启错误提示(默认开启)
     */
    errorMsg?: boolean
  }
}

// 定义常见http状态码错误
const httpStatus: Record<number, string> = {
  400: '请求参数错误',
  401: '授权状态失效，请重新登录',
  403: '服务器拒绝访问',
  404: '404 Not Found',
  405: '请求方法不允许',
  408: '请求超时',
  500: '服务器内部错误',
  501: '服务未实现',
  502: '网关错误',
  503: '服务不可用',
  504: '网关超时',
  505: 'HTTP版本不受支持'
}

interface Options {
  base: AxiosRequestConfig
  interceptors?: {
    request?: (config: AxiosRequestConfig) => void | Promise<any>
    response?: (response: AxiosResponse) => void | Promise<any>
    responseError?: (response: AxiosResponse) => void | Promise<any>
  }
  loadingService?: {
    loadingStart: () => void
    loadingClose: () => void
  }
}

export class BaseService {
  private http: AxiosInstance
  constructor() {
    this.http = axios.create(options.base)
    this.http.interceptors.request.use(
      async (config) => {
        try {
          await options.interceptors?.request?.(config)
          config.loading !== false && options.loadingService?.loadingStart()
          config.repeat_cancel && cancelPendingRequest.removePending(config)
          config.cache_task && cacheTaskResponse.addTask(config)
          cancelPendingRequest.addPending(config)
          return config
        } catch (error) {
          return Promise.reject(error)
        }
      },
      (error) => {
        return Promise.reject(error)
      }
    )
    this.http.interceptors.response.use(
      async (response) => {
        try {
          response.config.loading !== false &&
            options.loadingService?.loadingClose()
          response.config.repeat_cancel && cancelPendingRequest.removePending(response.config)
          await options.interceptors?.response?.(response)
          if (response.config.cache_task) {
            return new Promise((resolve) => {
              const cache_task_config = response.config.cache_task_config
              response.config.cache_task_config = {
                ...cache_task_config,
                status: true,
                resolve,
                data: response.data
              }
              cacheTaskResponse.removeTask(response.config)
            })
          }
          return response.data
        } catch (error) {
          return Promise.reject(error)
        }
      },
      async (error) => {
        error.config?.loading !== false &&
          options.loadingService?.loadingClose()
        error.config && cancelPendingRequest.removePending(error.config)
        error.config && cacheTaskResponse.removeTaskItem(error.config)
        if (error.response) {
          await options.interceptors?.responseError?.(error.response)
          // 请求已发出，服务器使用状态代码进行响应
          // 超出 2xx 的范围
          return this.retryRequest(error)
        }
        if (axios.isCancel(error)) {
          return Promise.reject(new Error(`当前请求已取消：\n${error.message}`))
        }
        return Promise.reject(error)
      }
    )
  }
  /**
   * 请求重试
   */
  retryRequest(error: any) {
    const config = error.config
    if (!config || !config.retryTimes) {
      return this.handleReject(error)
    }
    const { retryCount = 0, retryDelay = 300, retryTimes = 0 } = config
    // 在请求对象上设置重试次数
    config.retryCount = retryCount
    // 判断是否超过了重试次数
    if (retryCount >= retryTimes) {
      return Promise.reject(error)
    }
    // 增加重试次数
    config.retryCount++
    // 延时处理
    const delay = new Promise<void>((resolve) => {
      setTimeout(() => {
        resolve()
      }, retryDelay)
    })
    // 重新发起请求
    return delay.then(() => {
      return this.http(config)
    })
  }
  /**
   * 抛出请求异常
   */
  handleReject(error: any) {
    const response: AxiosResponse = error.response
    //进行全局错误提示
    if (response.data.detail) {
      //如果后端返回了具体错误内容
      error.config?.errorMsg !== false &&
        useSingleMsg('error', response.data.detail)
      return Promise.reject(error)
    }
    if (response.status && httpStatus[response.status]) {
      // 存在错误状态码
      error.config?.errorMsg !== false &&
        useSingleMsg('error', httpStatus[response.status])
      return Promise.reject(error)
    }
    //如果没有具体错误内容，找后端
    console.error(`后端接口未按照约定返回，请注意：\n${response.config.url}`)
    return Promise.reject(new Error('未知错误，请稍后再试'))
  }

  protected request<R>(
    method: Method,
    path: string,
    body?: any,
    ext?: ExtOptions
  ): Promise<R> {
    return this.http.request<any, R, any>({
      url: path,
      method,
      params: ['get', 'delete'].includes(method) ? body : undefined,
      data: ['post', 'put'].includes(method) ? body : undefined,
      ...ext
    })
  }
}

export type ExtOptions = AxiosRequestConfig

const {VITE_APP_SERVER_URL, MODE, VITE_APP_MOCK_URL} = import.meta.env
const options: Options = {
  base: {
    baseURL: VITE_APP_SERVER_URL
  },
  interceptors: {
    request: (config) => {
      const token = storage.get('userInfo')?.token
      token && ((config.headers as any).Authorization = `Bearer ${token}`)
      if(MODE === 'development' && (mockUrls as string[]).includes(config.url!)) {
        config.baseURL = VITE_APP_MOCK_URL
      }
    },
    responseError(errorResponse) {
      if (errorResponse.status === 401) {
        router.replace({
          name: 'login'
        })
      }
    }
  },
  loadingService
}
