import router from "@/router";
import storage from "@/utils/storage";
import server from "@/_config/server";
import loading from "@/_packages/request/loading";
import { RequestOptions } from "@/_packages/request/utils/request";
import axios, {
  AxiosInstance,
  Method,
  AxiosRequestConfig,
  AxiosResponse,
  ResponseType,
} from "axios";

// 定义常见http状态码错误
const httpStatus: { [key: number]: string } = {
  400: "请求参数错误",
  401: "未授权，请登录",
  403: "服务器拒绝访问",
  404: "404 Not Found",
  405: "请求方法不允许",
  408: "请求超时",
  500: "服务器内部错误",
  501: "服务未实现",
  502: "网关错误",
  503: "服务不可用",
  504: "网关超时",
  505: "HTTP版本不受支持",
};

export class BaseService {
  private http: AxiosInstance;
  queue: any[];
  constructor() {
    this.http = axios.create(options.base);
    this.queue = [];

    this.http.interceptors.request.use(
      async (config) => {
        try {
          /** 自定义请求拦截器 */
          await options.interceptors?.request?.(config);
          /** 如果配置了loading */
          if (config.loading !== false && !config.retryActiveCount) {
            options.loading?.loadingStart();
          }
          this.addQueue(config);
          return config;
        } catch (e) {
          // 捕获代码错误
          return Promise.reject(e);
        }
      },
      (error) => {
        return Promise.reject(error);
      }
    );
    this.http.interceptors.response.use(
      async (response) => {
        try {
          /** 自定义响应拦截器 */
          await options.interceptors?.response?.(response);
          await options.loading?.loadingClose();
          this.removeQueue(response.config);
          return response.data;
        } catch (e) {
          /** 如果捕获到代码异常，直接reject */
          if (e instanceof Error) {
            return Promise.reject(e);
          }
          return this.retryRequest(response);
        }
      },
      async (error) => {
        if (error?.response) {
          /** 执行错误拦截器钩子 */
          try {
            await options.interceptors?.responseError?.(error.response);
            //如果HTTP状态码非200，并且有返回内容
            return this.retryRequest(error.response);
          } catch (e) {
            /** 如果捕获到代码异常，直接reject */
            if (e instanceof Error) {
              return Promise.reject(e);
            }
            return this.retryRequest(error.response);
          }
        } else {
          if (axios.isCancel(error)) {
            //主动取消请求
            return Promise.reject(
              new Error(`当前请求已取消：\n${error.message}`)
            );
          } else {
            options.loading?.showToast?.(error?.message || error + "");
            return Promise.reject(error?.message || error + "" || "未知错误");
          }
        }
      }
    );
  }
  /** 添加请求队列 */
  addQueue(config: AxiosRequestConfig) {
    this.queue.forEach((item, index) => {
      // 如果存在相同的请求，进入判断
      if (item.url === config.url && item.method === config.method) {
        if (item.cancel && item.queue) {
          item.cancel(JSON.stringify(item));
        }
        // 如果存在相同的请求，从请求队列中移除旧的请求
        this.queue.splice(index, 1);
      }
    });
    config.cancelToken = new axios.CancelToken((c) => {
      //添加队列
      this.queue.push({
        url: config.url,
        method: config.method,
        params: config.params,
        data: config.data,
        cancel: c,
        //默认允许取消请求
        queue: config.queue ?? true,
      });
    });
  }
  /** 移出请求队列 */
  removeQueue(config: AxiosRequestConfig) {
    const index = this.queue.findIndex((item) => {
      return item.url === config.url && item.method === config.method;
    });
    if (index !== -1) {
      this.queue.splice(index, 1);
    }
  }
  /** 请求重试 */
  retryRequest(response: AxiosResponse) {
    const config = response.config;
    if (config.retryActiveCount === undefined) {
      //设置当前重试第几次，默认0
      config.retryActiveCount = 0;
    }
    if (config.retryCount === undefined) {
      //设置重置最大次数，默认3
      config.retryCount = 3;
    }
    if (config.retryActiveCount >= config.retryCount || config.retry !== true) {
      this.removeQueue(response.config);
      return this.handleReject(response);
    }
    config.retryActiveCount += 1;
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(this.http(response.config));
      }, config.retryDelay || 100);
    });
  }
  /** 抛出请求异常 */
  async handleReject(response: AxiosResponse) {
    await options.loading?.loadingClose();
    this.removeQueue(response.config);
    //进行全局错误提示
    if (response.data.detail) {
      //如果后端返回了具体错误内容
      options.loading?.showToast?.(response.data.detail);
      return Promise.reject(response.data);
    }
    if (response.status && httpStatus[response.status]) {
      // 存在错误状态码
      options.loading?.showToast?.(httpStatus[response.status]);
      return Promise.reject(response.data);
    }
    //如果没有具体错误内容，找后端
    console.error(`后端接口未按照约定返回，请注意：\n${response.config.url}`);
    await options.loading?.showToast?.("未知错误，请稍后再试");
    return Promise.reject(new Error("未知错误，请稍后再试"));
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
      params: ["get"].includes(method) ? body : undefined,
      data: ["post", "put", "patch"].includes(method) ? body : undefined,
      queue: ext?.queue,
      retry: ext?.retry,
      loading: ext?.loading,
      baseURL: ext?.baseUrl || options.base.baseURL,
      responseType: ext?.responseType,
    });
  }
}

export interface ExtOptions {
  queue?: boolean | undefined;
  retry?: boolean | undefined;
  loading?: boolean | undefined;
  baseUrl?: string | null;
  responseType?: ResponseType;
}

const options: RequestOptions = {
  base: {
    baseURL: server.baseUrl,
  },
  loading,
  interceptors: {
    request: (config) => {
      const token = storage.get("accountInfo")?.token;
      token && ((config.headers as any).Authorization = `Bearer ${token}`);
      return Promise.resolve(config);
    },
    responseError(errorResponse) {
      if (errorResponse.status === 401) {
        router.replace({
          name: "login",
        });
      }
      return Promise.reject("未授权，请登录");
    },
  },
};
