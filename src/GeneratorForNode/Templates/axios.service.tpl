import axios from 'axios'
import axiosCancel from '@/_packages/request/cancel'
import axiosRetry from '@/_packages/request/retry'
import loadingService from '@/_packages/request/loading'
import { useSingleMsg } from '@/hooks/utils'
import router from '@/router'
import { storage } from '@/utils'
import mockUrls from '@/_config/mock.json'
import useAppStore from '@/store/app'

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  Method
} from 'axios'

declare module 'axios' {
  interface AxiosRequestConfig {
    /**
     * 开启loading(默认开启)
     */
    loading?: boolean
    /**
     * 开启重试(默认关闭)
     */
    retry?: boolean
    /**
     * 重试次数(默认3次)
     * @description 请勿手动设置
     */
    __retryCount?: number
    /**
     * @description 取消请求(默认关闭)
     * @description 默认url+method相同为判断依据;
     * @description 采用url+method+param+data相同为判断依据(请使用对象模式且设置repeat为true)
     */
    cancel?: boolean | {
      repeat?: boolean
    }
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
  415: '服务器无法处理请求中所包含的媒体类型',
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
    request?: (config: AxiosRequestConfig) => void
    response?: (response: AxiosResponse) => void
    responseError?: (response: AxiosResponse) => void
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
       (config) => {
        if(config.loading !== false && config.__retryCount === undefined) {
          options.loadingService?.loadingStart()
        }
        axiosCancel.addPending(config)
        options.interceptors?.request?.(config)
        return config
      },
      (error) => {
        return Promise.reject(error)
      }
    )
    this.http.interceptors.response.use(
       (response) => {
        response.config.loading !== false && options.loadingService?.loadingClose()
        axiosCancel.removePending(response.config)
        options.interceptors?.response?.(response)
        return response.data
      },
      (error) => {
        if (axios.isCancel(error)) {
          const cancelError = error as any
          cancelError.config.loading !== false && options.loadingService?.loadingClose()
          return Promise.reject(new Error(`${(cancelError as any).config.url} => 请求被取消`))
        }
        if (error.response) {
          axiosCancel.removePending(error.config)
          options.interceptors?.responseError?.(error.response)
          if(error.config.retry) {
            return axiosRetry.run(this.http, error).catch(this.handleReject)
          }
          return this.handleReject(error)
        }
        error.config.loading !== false && options.loadingService?.loadingClose()
        return Promise.reject(error)
      }
    )
  }
  /**
   * 抛出请求异常
   */
  async handleReject(error: any) {
    error.config.loading !== false && options.loadingService?.loadingClose()
    const response: AxiosResponse = error.response
    if(response.config.responseType === 'blob') {
      const data = await this.blobToJson(response.data)
      response.data = data
    }
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
  blobToJson(blob: any) {
    return new Promise((resolve) => {
      const reader = new FileReader()
      reader.onload = () => {
        try {
          const data = JSON.parse(reader.result as string)
          resolve(data)
        } catch (error) {
          resolve({})
        }
      }
      reader.readAsText(blob)
    })
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
      token && ((config.headers as any).Authorization = `Bearer #@token#`)
      if(MODE === 'development' && (mockUrls as string[]).includes(config.url!)) {
        config.baseURL = VITE_APP_MOCK_URL
      }
    },
    responseError(errorResponse) {
      if (errorResponse.status === 401) {
        const appStore = useAppStore()
        appStore.removeUserInfo()
        router.replace({
          name: 'login'
        })
      }
    }
  },
  loadingService
}
