import storage from "@/utils/storage";
import server from "@/_config/server";
import loading from "@/_packages/request/loading";
import cancelRequest from "@/_packages/request/utils/cancel-request";
import { RequestOptions } from "@/_packages/request/utils/request";
import retryRequest from "@/_packages/request/utils/retry-request";
import axios, { AxiosInstance, Method } from "axios";
import { ElMessage } from "element-plus";
import store from "@/store";

export class BaseService {
    private http: AxiosInstance
    constructor() {
        this.http = axios.create(options.base);
        this.http.interceptors.request.use(
            (config) => {
                options.loading?.loadingStart();
                options.interceptors?.request?.(config);
                cancelRequest.cancel(config);
                cancelRequest.add(config, axios.CancelToken);
                retryRequest.add(config);
                return config;
            },
            (error) => {
                options.loading?.loadingClose();
                cancelRequest.remove(error.config);
                retryRequest.remove(error.config);
                return Promise.reject(error);
            }
        );
        this.http.interceptors.response.use(
            (response) => {
                options.loading?.loadingClose();
                options.interceptors?.response?.(response);
                cancelRequest.remove(response.config);
                retryRequest.remove(response.config);
                return response.data;
            },
            async (error) => {
                //如果HTTP状态码非200，并且有返回内容
                if (error.response) {
                    options.interceptors?.responseError?.(error.response);
                    if (error.response.config.retry) {
                        return retryRequest
                            .start(error.response, this.http)
                            .finally(() => {
                                options.loading?.loadingClose();
                            });
                    } else {
                        options.loading?.loadingClose();
                    }
                    return Promise.reject(error);
                } else {
                    if (axios.isCancel(error)) {
                        return Promise.reject(`已取消请求信息：${error.message}`);
                    }
                    return Promise.reject(error);
                }
            }
        );
    }

    protected request<R>(method: Method, path: string, body?: any): Promise<R> {
        return this.http.request<any, R, any>(
            {
                url: path,
                method,
                data: body
            });
    }

}



const transformRequestUrl = function (url: string, params: any) {
    const _url = url.replace(
        /{([^}]+)}.*?/g,
        function (match: string, $1: string) {
            const value = params[$1];
            delete params[$1];
            return value;
        }
    );
    return {
        tUrl: _url,
        tParams: params,
    };
};

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

const options: RequestOptions = {
    base: {
        baseURL: server.baseUrl,
        headers: {
            Authorization: `Bearer ${storage.get("token")}`,
        },
    },
    loading,
    interceptors: {
        request: (config) => {
            const { url, data, params } = config as any;
            const { tUrl } = transformRequestUrl(url, data || params);
            config.url = tUrl;
        },
        response: (response) => { },
        responseError: (errorResponse: any) => {
            const status = errorResponse.status;
            ElMessage.error(httpStatus[status]);
            if (status === 401) {
                store.dispatch("account/logout");
            }
        },
    },
};