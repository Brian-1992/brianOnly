Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T2GetExcel = '../../../api/AA0143/ReportExcel';
    var T1Name = "緊急醫療出貨申請日報表";

    var T1Rec = 0;
    var T1LastRec = null;

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //院內碼
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0143/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    //入庫庫房Store
    var st_TOWH = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0143/GetTOWHcombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    //廠商名稱
    var st_AGEN = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0143/GetAgenCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    //狀態Store
    var st_STATUS = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0143/GetStatusCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },        // 緊急醫療出貨單編號
            { name: 'MMCODE', type: 'string' },         // 院內碼
            { name: 'MMNAME_C', type: 'string' },       // 中文品名
            { name: 'MMNAME_E', type: 'string' },       // 英文品名  
            { name: 'APPQTY', type: 'string' },         // 申請數量
            { name: 'BASE_UNIT', type: 'string' },      // 計量單位(包裝單位)
            { name: 'TOWH', type: 'string' },           // 入庫庫房
            { name: 'REQDATE', type: 'string' },        // 要求到貨日期
            { name: 'CR_UPRICE', type: 'string' },      // 單價
            { name: 'M_PAYKIND', type: 'string' },      // 收費屬性
            { name: 'AGEN_NAME', type: 'string' },      // 廠商名稱
            { name: 'INID', type: 'string' },           // 庫房責任中心
            { name: 'INID_NAME', type: 'string' },      // 責任中心名稱
            { name: 'APPID', type: 'string' },          // 申請人ID
            { name: 'APPNM', type: 'string' },          // 申請人名稱
            { name: 'RPTDATE', type: 'string' },        // 報表日期
            { name: 'APPTIME', type: 'string' },        // 申請時間
            { name: 'EMAIL', type: 'string' },          // 廠商Email
            { name: 'CR_STATUS', type: 'string' },      // 狀態
            { name: 'WH_NAME', type: 'string' },        // 庫房名稱
            { name: 'AGEN_NO', type: 'string' },        // 廠商代碼
            { name: 'STATUS', type: 'string' },         // 狀態(中文)
            { name: 'APP_AMOUNT', type: 'string' },     // 申請金額            
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'CRDOCNO', direction: 'ASC' }],//依CRDOCNO排序
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0143/GetAll',  // 呼叫AA0143Control 中的GetAll,再至DB取得資料值 
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: { //觸發
            beforeload: function (store, options) {
                var np = { //前端取得參數
                    p0: T1Query.getForm().findField('P0').getValue(),     //MMCODE
                    p1: T1Query.getForm().findField('P1').getValue(),     //AGEN_NO
                    p2: T1Query.getForm().findField('P2').getValue(),     //TOWH
                    p3: T1Query.getForm().findField('P3').getRawValue(),  //APPTIME_Start
                    p4: T1Query.getForm().findField('P4').getRawValue(),  //APPTIME_End
                    p5: T1Query.getForm().findField('P5').getValue(),     //CR_STATUS
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    var mLabelWidth = 80;
    var mWidth = 250;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 4 4 0',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                T1QueryMMCode, //院內碼
                {
                    xtype: 'combo',
                    fieldLabel: '廠商名稱',
                    store: st_AGEN,
                    id: 'P1',
                    name: 'P1',
                    queryMode: 'local',
                    allowBlank: true,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>' //前端格式化可折行
                }, {
                    xtype: 'combo',
                    fieldLabel: '入庫庫房',
                    store: st_TOWH,
                    id: 'P2',
                    name: 'P2',
                    queryMode: 'local',
                    allowBlank: true,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>' //前端格式化可折行
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '申請日期',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 21,
                    width: 160,
                    allowBlank: false,
                    fieldCls: 'required',
                    value: Ext.util.Format.date(new Date(), "Y-m-") + "01",
                    regexText: '請選擇日期'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    id: 'P4',
                    width: 90,
                    labelWidth: 8,
                    allowBlank: false,
                    value: new Date(),
                    fieldCls: 'required',
                    regexText: '請選擇日期'
                }, {
                    xtype: 'combo',
                    fieldLabel: '狀態',
                    store: st_STATUS,
                    id: 'P5',
                    name: 'P5',
                    queryMode: 'local',
                    allowBlank: false,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    fieldCls: 'required',
                    value: 'B' //預設值
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        
                        if (this.up('form').getForm().isValid() == false) {
                            Ext.Msg.alert('提醒', '請填寫必填欄位');
                            return;
                        }
                        T1Load(true);
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('MMCODE').focus(); //進入畫面時輸入遊標預設在MMCODE
                        msglabel('');
                    }
                }, {
                    xtype: 'button',
                    text: '日報表',
                    handler: function () {
                        ReportWindow.setTitle('日報表');
                        ReportWindow.show();
                    }
                }
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true, //列示資料筆數
        border: false,
        plain: true,
        buttons: [
            {
                text: '產生通知單',
                itemId: 'Order',
                id: 'btnOrder',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('產生通知單', '是否產生通知單？', function (btn, text) {
                        
                        if (btn === 'yes') {
                            //var myMask = new Ext.LoadMask(Ext.getCmp('T1Grid'), { msg: '處理中...' });
                            //myMask.show();
                            Ext.Ajax.request({
                                url: '/api/AA0143/Order',
                                method: reqVal_p,
                                params: {
                                    crdocno: T1LastRec.data.CRDOCNO //醫緊急療出貨單編號
                                },
                                success: function (response) {
                                    // myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T1Load(true);
                                        msglabel('已產生訂單');
                                    }
                                    else {
                                        console.log("產生訂單失敗");
                                    }
                                },
                                failure: function (response, options) {

                                }
                            })
                        }
                    })
                }
            }, {
                text: '重新讀取廠商EMAL',
                itemId: 'ReadMail',
                id: 'btnReadMail',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('重新讀取廠商EMAL', '是否重新讀取廠商EMAL？', function (btn, text) {
                        if (btn === 'yes') {
                            
                            Ext.Ajax.request({
                                url: '/api/AA0143/ReadEmail',
                                method: reqVal_p,
                                params: {
                                    crdocno: T1LastRec.data.CRDOCNO //醫緊急療出貨單編號
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T1Load(true);
                                        msglabel('重新讀取廠商EMAL成功');
                                    }
                                    else {
                                        console.log("重新讀取廠商EMAL失敗");
                                    }
                                },
                                failure: function (response, options) {

                                }
                            })
                        }
                    })

                }
            }, {
                text: '重新寄信',
                itemId: 'SendMail',
                id: 'btnSendMail',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('重新寄信', '是否重新寄信？', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0143/SendMail',
                                method: reqVal_p,
                                params: {
                                    crdocno: T1LastRec.data.CRDOCNO //醫緊急療出貨單編號
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T1Load(true);
                                        msglabel('重新寄信成功');
                                    }
                                    else {
                                        console.log("重新寄信失敗");
                                    }
                                },
                                failure: function (response, options) {

                                }
                            })
                        }
                    })

                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "緊急醫療出貨單編號",
            dataIndex: 'CRDOCNO',
            width: 130
            }, {
                text: "狀態",
                dataIndex: 'STATUS', //狀態（中文)
                width: 130
            }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 130
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 130
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 130
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            width: 65
        }, {
            text: "計量單位",
            dataIndex: 'BASE_UNIT',
            width: 90
        }, {
            text: "入庫庫房",
            dataIndex: 'WH_NAME',
            width: 130
        }, {
            text: "要求到貨日期",
            dataIndex: 'REQDATE',
            width: 90
        }, {
            text: "單價",
            dataIndex: 'CR_UPRICE',
            width: 90
        }, {
            text: "收費屬性",
            dataIndex: 'M_PAYKIND',
            width: 130
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAME',
            width: 130
        }, {
            text: "庫房責任中心",
            dataIndex: 'INID',
            width: 130
        }, {
            text: "申請人",
            dataIndex: 'APPNM',
            width: 100
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME',
            width: 100
        }, {
            text: "廠商Email",
            dataIndex: 'EMAIL',
            width: 130
        }, {
            text: "產生通知單時間",
            dataIndex: 'ORDERTIME', 
            width: 130
        }, {
            text: "寄EMAIL時間",
            dataIndex: 'EMAILTIME', 
            width: 130
        }, {
            text: "收信確認時間",
            dataIndex: 'REPLYTIME', 
            width: 130
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                if (T1Rec > 0) {
                    T1LastRec = records[0]; //將游標所在資料寫至T1LastRec
                    
                    if (T1LastRec.data.CR_STATUS == 'B') {
                        Ext.getCmp('btnOrder').enable();
                        Ext.getCmp('btnReadMail').enable();
                        Ext.getCmp('btnSendMail').disable();
                    }
                    if ((T1LastRec.data.CR_STATUS == 'F') || (T1LastRec.data.CR_STATUS == 'H')) {
                        Ext.getCmp('btnOrder').disable();
                        Ext.getCmp('btnReadMail').disable();
                        Ext.getCmp('btnSendMail').enable();
                    }
                }
            }
        }
    });

    function T1Load(moveFirst) {
        if (moveFirst) {
            T1Tool.moveFirst(); //移動到第一頁，與按一下“first”按鈕有相同的效果。
        }
        else {
            T1Store.load({
                params: {
                    start: 0 //start: 0 從第0筆開始顯示,如果要從後端控制每個分頁起始, 可從這邊傳給後端
                }
            });
        }
        Ext.getCmp('btnOrder').disable();
        Ext.getCmp('btnReadMail').disable();
        Ext.getCmp('btnSendMail').disable();
    }

    // --------日報表視窗------------------
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'APPTIME', direction: 'ASC' }],//依APPTIME排序
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0143/GetReport',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 將申請時間代入參數
                var np = {
                    p6: T2Query.getForm().findField('P6').getRawValue(),  //APPTIME_Start
                    p7: T2Query.getForm().findField('P7').getRawValue(),  //APPTIME_End
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯出是否disable
                
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('btnExcel').enable();
                } else {
                    Ext.getCmp('btnExcel').disable();
                }
            }
        }
    });

    function T2Load() {
        T2Tool.moveFirst();
    }

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
        },
        items: [{
            xtype: 'datefield',
            fieldLabel: '報表日期',
            name: 'P6',
            id: 'P6',
            enforceMaxLength: true,
            maxLength: 21,
            labelWidth: 80,
            width: 160,
            allowBlank: false,
            value: new Date(),
            //value: Ext.util.Format.date(new Date(), "Y-m-") + "01",
            regexText: '請選擇日期'
        }, {
            xtype: 'datefield',
            fieldLabel: '至',
            labelSeparator: '',
            name: 'P7',
            id: 'P7',
            width: 90,
            labelWidth: 8,
            allowBlank: false,
            value: new Date(),
            regexText: '請選擇日期'
        }, {
            xtype: 'button',
            text: '查詢',
            handler: function () {
                T2Load(true);
                msglabel('');
            }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P6').focus(); //進入畫面時輸入游標預設在申請日期起
                msglabel('');
            }
        }]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '匯出',
                id: 'btnExcel',
                disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '緊急醫療出貨申請日報表.xlsx' });
                    p.push({ name: 'start', value: T2Query.getForm().findField('P6').getRawValue() });   //申請時間起
                    p.push({ name: 'end', value: T2Query.getForm().findField('P7').getRawValue() });   //申請時間迄
                    PostForm(T2GetExcel, p);
                    msglabel('匯出完成');
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        height: windowHeight - 60,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "報表日期",
                dataIndex: 'RPTDATE',
                width: 100
            }, {
                text: "申請時間",
                dataIndex: 'APPTIME',
                width: 100
            }, {
                text: "緊急醫療出貨單編號",
                dataIndex: 'CRDOCNO',
                width: 130
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 130
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 130
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 130
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 90
            }, {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAME',
                width: 130
            }, {
                text: "單價",
                dataIndex: 'CR_UPRICE',
                width: 90
            }, {
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 65
            }, {
                text: "申請金額",
                dataIndex: 'APP_AMOUNT',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val, meta, record) { return Ext.util.Format.number(val, "0,000.00"); }
            }, {
                text: "收費屬性",
                dataIndex: 'M_PAYKIND',
                width: 130
            }, {
                text: "入庫庫房",
                dataIndex: 'WH_NAME',
                width: 130
            }, {
                text: "庫房責任中心",
                dataIndex: 'INID',
                width: 130
            }, {
                text: "責任中心名稱",
                dataIndex: 'INID_NAME',
                width: 130
            }, {
                text: "申請人",
                dataIndex: 'APPNM',
                width: 100
            }, {
                header: "",
                flex: 1
            }
        ]
    });

    var ReportWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    {
                        xtype: 'panel',
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        border: false,
                        items: [T2Grid]
                    }
                ],
            }
        ],
        width: "1000px",
        height: windowHeight,
        resizable: false,
        draggable: true,
        closable: false,
        y: 0,
        title: "盤點明細管理",
        buttons: [{
            text: '關閉',
            handler: function () {
                ReportWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                ReportWindow.setY(0);
            }
        }
    });
    ReportWindow.hide();
    // --------------------------

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body, //始終渲染在頁面
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false, //是否可以收縮
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

});
