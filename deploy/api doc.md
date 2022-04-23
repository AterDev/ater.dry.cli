### [AuthResult](#AuthResult)
|字段名|类型|必须|说明|
|-|-|-|-|
|id|string|false|-|
|username|string|false|-|
|role|string|false|-|
|token|string|false|-|

### [LoginDto](#LoginDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|password|string|false|-|

### [PageResultOfCustomerItemDto](#PageResultOfCustomerItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|CustomerItemDto[]|true|-|
|pageIndex|number|false|-|

### [CustomerItemDto](#CustomerItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|wxOpenId|string|true|-|
|wxUnionId|string|true|-|
|wxName|string|true|-|
|wxAvatar|string|true|微信头像|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [Status](#Status)
|字段名|类型|必须|说明|
|-|-|-|-|

### [CustomerFilter](#CustomerFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|realName|string|true|真实姓名|
|idNumber|string|true|身份证|

### [FilterBase](#FilterBase)
|字段名|类型|必须|说明|
|-|-|-|-|
|pageIndex|number|false|-|
|pageSize|number|false|-|

### [Customer](#Customer)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|wxOpenId|string|true|-|
|wxUnionId|string|true|-|
|wxName|string|true|微信昵称|
|wxAvatar|string|true|微信头像|
|customerInvoiceInfo|CustomerInvoiceInfo|true|发票信息|
|personInfos|PersonInfo[]|true|关联的人员档案|
|reservations|Reservation[]|true|预约|
|orders|Order[]|true|订单|
|payRecords|PayRecord[]|true|支付记录|
|refundRecords|RefundRecord[]|true|退款记录|

### [CustomerInvoiceInfo](#CustomerInvoiceInfo)
|字段名|类型|必须|说明|
|-|-|-|-|
|invoiceName|string|false|抬头名称|
|number|string|false|税号|
|userName|string|false|-|
|userPhone|string|false|-|
|email|string|true|-|
|invoiceType|InvoiceType|false|发票类型|
|customerId|string|false|-|
|customer|Customer|false|-|

### [InvoiceType](#InvoiceType)
|字段名|类型|必须|说明|
|-|-|-|-|

### [EntityBase](#EntityBase)
|字段名|类型|必须|说明|
|-|-|-|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [PersonInfo](#PersonInfo)
|字段名|类型|必须|说明|
|-|-|-|-|
|realName|string|false|真实姓名|
|idNumber|string|false|-|
|phone|string|false|-|
|idType|IdType|false|证件类型;;0=IDCard|
|sex|SexType|false|性别;;0=Male;1=Female;2=Else|
|country|string|true|国家|
|address|string|true|地址|
|isSelf|boolean|false|是否本人|
|relation|string|true|关系|
|customer|Customer|false|客户信息|
|reservations|Reservation[]|true|预约信息|

### [IdType](#IdType)
|字段名|类型|必须|说明|
|-|-|-|-|

### [SexType](#SexType)
|字段名|类型|必须|说明|
|-|-|-|-|

### [Reservation](#Reservation)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|检测项目名称|
|number|string|true|预约编号,自动生成|
|remark|string|true|备注|
|reserveStatus|ReserveStatus|false|状态|
|reserveTime|string|false|预约时间|
|detectProcessing|string|true|检测进度|
|reservationType|ReservationType|false|检测类型，如单人单管|
|customer|Customer|false|-|
|personInfo|PersonInfo|false|-|
|order|Order|true|-|

### [ReserveStatus](#ReserveStatus)
|字段名|类型|必须|说明|
|-|-|-|-|

### [ReservationType](#ReservationType)
|字段名|类型|必须|说明|
|-|-|-|-|

### [Order](#Order)
|字段名|类型|必须|说明|
|-|-|-|-|
|content|string|true|订单内容|
|price|number|false|订单价格|
|number|string|false|订单号|
|orderType|OrderStatus|false|订单状态|
|customer|Customer|false|客户信息|
|reservationId|string|false|-|
|reservation|Reservation|false|关联的预约|

### [OrderStatus](#OrderStatus)
|字段名|类型|必须|说明|
|-|-|-|-|

### [PayRecord](#PayRecord)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|customer|Customer|false|-|

### [RefundRecord](#RefundRecord)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|customer|Customer|false|-|

### [PageResultOfCustomerInvoiceInfoItemDto](#PageResultOfCustomerInvoiceInfoItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|CustomerInvoiceInfoItemDto[]|true|-|
|pageIndex|number|false|-|

### [CustomerInvoiceInfoItemDto](#CustomerInvoiceInfoItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|invoiceName|string|false|抬头名称|
|number|string|false|税号|
|userName|string|false|-|
|userPhone|string|false|-|
|email|string|true|-|
|invoiceType|InvoiceType|false|发票类型|
|customerId|string|false|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [CustomerInvoiceInfoFilter](#CustomerInvoiceInfoFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|invoiceName|string|true|抬头名称|
|number|string|true|税号|
|userName|string|true|-|
|userPhone|string|true|-|
|invoiceType|InvoiceType|true|发票类型|
|customerId|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [CustomerInvoiceInfoAddDto](#CustomerInvoiceInfoAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|invoiceName|string|false|抬头名称|
|number|string|false|税号|
|userName|string|false|-|
|userPhone|string|false|-|
|email|string|true|-|
|invoiceType|InvoiceType|false|发票类型|
|customerId|string|false|-|
|status|Status|false|状态|

### [CustomerInvoiceInfoUpdateDto](#CustomerInvoiceInfoUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|invoiceName|string|true|抬头名称|
|number|string|true|税号|
|userName|string|true|-|
|userPhone|string|true|-|
|email|string|true|-|
|invoiceType|InvoiceType|true|发票类型|
|customerId|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [PageResultOfDetectRecordItemDto](#PageResultOfDetectRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|DetectRecordItemDto[]|true|-|
|pageIndex|number|false|-|

### [DetectRecordItemDto](#DetectRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|openTime|Date|false|开门时间|
|leaveTime|Date|false|离开时间|
|isFaceVerify|boolean|false|是否人脸验证|
|isGetMaterials|boolean|false|是否获取物料|
|orderId|string|false|-|
|roomId|string|false|-|
|personInfoId|string|false|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [DetectRecordFilter](#DetectRecordFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|openTime|Date|true|开门时间|
|leaveTime|Date|true|离开时间|
|isFaceVerify|boolean|true|是否人脸验证|
|isGetMaterials|boolean|true|是否获取物料|
|orderId|string|true|-|
|roomId|string|true|-|
|personInfoId|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [DetectRecord](#DetectRecord)
|字段名|类型|必须|说明|
|-|-|-|-|
|openTime|Date|false|开门时间|
|leaveTime|Date|false|离开时间|
|isFaceVerify|boolean|false|是否人脸验证|
|isGetMaterials|boolean|false|是否获取物料|
|orderId|string|false|-|
|order|Order|true|-|
|roomId|string|false|-|
|room|Room|true|-|
|customer|Customer|false|-|
|personInfoId|string|false|-|
|personInfo|PersonInfo|false|-|

### [Room](#Room)
|字段名|类型|必须|说明|
|-|-|-|-|
|number|string|false|编号|
|province|string|true|-|
|city|string|true|-|
|district|string|true|-|
|address|string|true|-|
|devices|Device[]|false|设备|

### [Device](#Device)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|

### [DetectRecordAddDto](#DetectRecordAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|openTime|Date|false|开门时间|
|leaveTime|Date|false|离开时间|
|isFaceVerify|boolean|false|是否人脸验证|
|isGetMaterials|boolean|false|是否获取物料|
|orderId|string|false|-|
|roomId|string|false|-|
|personInfoId|string|false|-|
|status|Status|false|状态|
|customerId|string|false|-|

### [DetectRecordUpdateDto](#DetectRecordUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|openTime|Date|true|开门时间|
|leaveTime|Date|true|离开时间|
|isFaceVerify|boolean|true|是否人脸验证|
|isGetMaterials|boolean|true|是否获取物料|
|orderId|string|true|-|
|roomId|string|true|-|
|personInfoId|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [PageResultOfDeviceItemDto](#PageResultOfDeviceItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|DeviceItemDto[]|true|-|
|pageIndex|number|false|-|

### [DeviceItemDto](#DeviceItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [DeviceFilter](#DeviceFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [DeviceAddDto](#DeviceAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|
|status|Status|false|状态|

### [DeviceUpdateDto](#DeviceUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [PageResultOfOrderItemDto](#PageResultOfOrderItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|OrderItemDto[]|true|-|
|pageIndex|number|false|-|

### [OrderItemDto](#OrderItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|price|number|false|订单价格|
|number|string|false|订单号|
|orderType|OrderStatus|false|订单状态|
|reservationId|string|false|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [OrderFilter](#OrderFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|price|number|true|订单价格|
|number|string|true|订单号|
|orderType|OrderStatus|true|订单状态|
|reservationId|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|用户的订单|

### [PageResultOfOrderRecordItemDto](#PageResultOfOrderRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|OrderRecordItemDto[]|true|-|
|pageIndex|number|false|-|

### [OrderRecordItemDto](#OrderRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|personName|string|false|-|
|idNumber|string|false|-|
|phone|string|false|-|
|idType|IdType|false|证件类型;;0=IDCard|
|sex|SexType|false|性别;;0=Male;1=Female;2=Else|
|country|string|true|国家|
|address|string|true|地址|
|name|string|true|检测项目名称|
|reserveNumber|string|true|预约编号,自动生成|
|remark|string|true|备注|
|price|number|false|订单价格|
|orderNumber|string|false|订单号|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [OrderRecordFilter](#OrderRecordFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|-|
|personName|string|true|-|
|idNumber|string|true|-|
|phone|string|true|-|
|idType|IdType|true|-|
|sex|SexType|true|-|
|price|number|true|订单价格|
|orderNumber|string|true|订单号|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [OrderRecord](#OrderRecord)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|personName|string|false|-|
|idNumber|string|false|-|
|phone|string|false|-|
|idType|IdType|false|证件类型;;0=IDCard|
|sex|SexType|false|性别;;0=Male;1=Female;2=Else|
|country|string|true|国家|
|address|string|true|地址|
|name|string|true|检测项目名称|
|reserveNumber|string|true|预约编号,自动生成|
|remark|string|true|备注|
|content|string|true|订单内容|
|price|number|false|订单价格|
|orderNumber|string|false|订单号|

### [PageResultOfPayRecordItemDto](#PageResultOfPayRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|PayRecordItemDto[]|true|-|
|pageIndex|number|false|-|

### [PayRecordItemDto](#PayRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [PayRecordFilter](#PayRecordFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|-|
|orderNumber|string|true|订单编号|
|price|number|true|支付金额|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [PayRecordAddDto](#PayRecordAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|status|Status|false|状态|
|customerId|string|false|-|

### [PayRecordUpdateDto](#PayRecordUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|-|
|orderNumber|string|true|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|true|支付金额|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [PageResultOfPermissionItemDto](#PageResultOfPermissionItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|PermissionItemDto[]|true|-|
|pageIndex|number|false|-|

### [PermissionItemDto](#PermissionItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|-|
|permissionPath|string|true|权限路径|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [PermissionFilter](#PermissionFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|parentId|string|true|-|

### [Permission](#Permission)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|-|
|parent|Permission|true|父级权限|
|permissionPath|string|true|权限路径|
|roles|Role[]|true|-|
|rolePermissions|RolePermission[]|true|-|

### [Role](#Role)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|角色名称|
|icon|string|true|图标|
|users|User[]|true|-|
|permissions|Permission[]|true|-|
|rolePermissions|RolePermission[]|true|-|

### [User](#User)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|用户名|
|realName|string|true|真实姓名|
|position|string|true|职位|
|email|string|true|-|
|emailConfirmed|boolean|false|-|
|passwordHash|string|false|-|
|passwordSalt|string|false|-|
|phoneNumber|string|true|-|
|phoneNumberConfirmed|boolean|false|-|
|twoFactorEnabled|boolean|false|-|
|lockoutEnd|Date|true|-|
|lockoutEnabled|boolean|false|-|
|accessFailedCount|number|false|-|
|lastLoginTime|Date|true|最后登录时间|
|retryCount|number|false|密码重试次数|
|avatar|string|true|头像url|
|roles|Role[]|true|-|

### [RolePermission](#RolePermission)
|字段名|类型|必须|说明|
|-|-|-|-|
|roleId|string|false|-|
|permissionId|string|false|-|
|permissionTypeMyProperty|PermissionType|false|权限类型|
|role|Role|false|-|
|permission|Permission|false|-|

### [PermissionType](#PermissionType)
|字段名|类型|必须|说明|
|-|-|-|-|

### [PermissionAddDto](#PermissionAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|-|
|permissionPath|string|true|权限路径|
|status|Status|false|状态|
|parentId|string|false|-|

### [PermissionUpdateDto](#PermissionUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|-|
|permissionPath|string|true|权限路径|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [PageResultOfPersonInfoItemDto](#PageResultOfPersonInfoItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|PersonInfoItemDto[]|true|-|
|pageIndex|number|false|-|

### [PersonInfoItemDto](#PersonInfoItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|realName|string|false|-|
|wxOpenId|string|false|微信openid|
|idNumber|string|false|-|
|phone|string|false|-|
|idType|IdType|false|证件类型;;0=IDCard|
|sex|SexType|false|性别;;0=Male;1=Female;2=Else|
|country|string|true|国家|
|address|string|true|地址|
|isSelf|boolean|false|是否本人|
|relation|string|true|关系|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [PersonInfoFilter](#PersonInfoFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|customerId|string|true|-|
|wxOpenId|string|true|微信openid|
|realName|string|true|-|
|idNumber|string|true|-|
|phone|string|true|-|
|sex|SexType|true|-|
|isSelf|boolean|true|是否本人|

### [PageResultOfRefundRecordItemDto](#PageResultOfRefundRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|RefundRecordItemDto[]|true|-|
|pageIndex|number|false|-|

### [RefundRecordItemDto](#RefundRecordItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [RefundRecordFilter](#RefundRecordFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|-|
|orderNumber|string|true|订单编号|
|price|number|true|支付金额|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [RefundRecordAddDto](#RefundRecordAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|-|
|orderNumber|string|false|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|false|支付金额|
|status|Status|false|状态|
|customerId|string|false|-|

### [RefundRecordUpdateDto](#RefundRecordUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|-|
|orderNumber|string|true|订单编号|
|orderContent|string|true|订单内容|
|content|string|true|支付内容|
|payType|string|true|支付方式|
|price|number|true|支付金额|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|

### [PageResultOfReservationItemDto](#PageResultOfReservationItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|ReservationItemDto[]|true|-|
|pageIndex|number|false|-|

### [ReservationItemDto](#ReservationItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|检测项目名称|
|number|string|true|预约编号,自动生成|
|remark|string|true|备注|
|reserveStatus|ReserveStatus|false|状态|
|detectProcessing|string|true|检测进度|
|reservationType|ReservationType|false|检测类型，如单人单管|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [ReservationFilter](#ReservationFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|reserveStatus|ReserveStatus|true|状态|
|reservationType|ReservationType|true|检测类型，如单人单管|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|customerId|string|true|-|
|personInfoId|string|true|-|
|orderId|string|true|-|

### [PageResultOfRoleItemDto](#PageResultOfRoleItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|RoleItemDto[]|true|-|
|pageIndex|number|false|-|

### [RoleItemDto](#RoleItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|角色名称|
|icon|string|true|图标|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [RoleFilter](#RoleFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|角色名称|
|status|Status|true|状态|
|userId|string|true|用户id|

### [RoleAddDto](#RoleAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|false|角色名称|
|icon|string|true|图标|

### [RoleUpdateDto](#RoleUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|name|string|true|角色名称|
|icon|string|true|图标|
|status|Status|true|状态|
|userIds|string[]|true|-|

### [PageResultOfRolePermissionItemDto](#PageResultOfRolePermissionItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|RolePermissionItemDto[]|true|-|
|pageIndex|number|false|-|

### [RolePermissionItemDto](#RolePermissionItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|roleId|string|false|-|
|permissionId|string|false|-|
|permissionTypeMyProperty|PermissionType|false|权限类型|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [RolePermissionFilter](#RolePermissionFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|roleId|string|true|-|
|permissionId|string|true|-|
|permissionTypeMyProperty|PermissionType|true|权限类型|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [RolePermissionAddDto](#RolePermissionAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|roleId|string|false|-|
|permissionId|string|false|-|
|permissionTypeMyProperty|PermissionType|false|权限类型|
|status|Status|false|状态|

### [RolePermissionUpdateDto](#RolePermissionUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|roleId|string|true|-|
|permissionId|string|true|-|
|permissionTypeMyProperty|PermissionType|true|权限类型|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [PageResultOfRoomItemDto](#PageResultOfRoomItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|RoomItemDto[]|true|-|
|pageIndex|number|false|-|

### [RoomItemDto](#RoomItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|number|string|false|编号|
|province|string|true|-|
|city|string|true|-|
|district|string|true|-|
|address|string|true|-|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [RoomFilter](#RoomFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|number|string|true|编号|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [RoomAddDto](#RoomAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|number|string|false|编号|
|province|string|true|-|
|city|string|true|-|
|district|string|true|-|
|address|string|true|-|
|status|Status|false|状态|

### [RoomUpdateDto](#RoomUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|number|string|true|编号|
|province|string|true|-|
|city|string|true|-|
|district|string|true|-|
|address|string|true|-|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|

### [VerifyResult](#VerifyResult)
|字段名|类型|必须|说明|
|-|-|-|-|
|isValide|boolean|false|-|
|message|string|false|-|
|code|number|false|用来查询错误信息|

### [PageResultOfUserItemDto](#PageResultOfUserItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|count|number|false|-|
|data|UserItemDto[]|true|-|
|pageIndex|number|false|-|

### [UserItemDto](#UserItemDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|用户名|
|realName|string|true|真实姓名|
|position|string|true|职位|
|email|string|true|-|
|emailConfirmed|boolean|false|-|
|phoneNumber|string|true|-|
|phoneNumberConfirmed|boolean|false|-|
|twoFactorEnabled|boolean|false|-|
|lockoutEnd|Date|true|-|
|lockoutEnabled|boolean|false|-|
|accessFailedCount|number|false|-|
|lastLoginTime|Date|true|最后登录时间|
|retryCount|number|false|密码重试次数|
|avatar|string|true|头像url|
|id|string|false|-|
|status|Status|false|状态|
|createdTime|Date|false|-|
|updatedTime|Date|false|-|
|isDeleted|boolean|false|软删除|

### [UserFilter](#UserFilter)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|用户名|
|email|string|true|-|
|realname|string|true|-|
|position|string|true|-|
|roleId|string|true|角色|

### [UserAddDto](#UserAddDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|false|用户名|
|realName|string|true|真实姓名|
|password|string|true|-|
|position|string|true|职位|
|email|string|true|-|
|phoneNumber|string|true|-|
|avatar|string|true|头像url|
|roleIds|string[]|true|-|

### [UserUpdateDto](#UserUpdateDto)
|字段名|类型|必须|说明|
|-|-|-|-|
|userName|string|true|用户名|
|realName|string|true|真实姓名|
|position|string|true|职位|
|password|string|true|-|
|email|string|true|-|
|emailConfirmed|boolean|true|-|
|phoneNumber|string|true|-|
|phoneNumberConfirmed|boolean|true|-|
|twoFactorEnabled|boolean|true|-|
|lockoutEnd|Date|true|-|
|lockoutEnabled|boolean|true|-|
|accessFailedCount|number|true|-|
|avatar|string|true|头像url|
|status|Status|true|状态|
|isDeleted|boolean|true|软删除|
|roleIds|string[]|true|-|

