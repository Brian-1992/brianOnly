Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAA = Ext.getUrlParam('isAA');

    var reportUrl = '/Report/A/AB0111.aspx';

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'REQDATE', type: 'string' },
            { name: 'DRNAME', type: 'string' },
            { name: 'PATIENTNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'CR_UPRICE', type: 'string' },
            { name: 'M_PAYKIND', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'INID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'EMAIL', type: 'string' },
            { name: 'WEXP_ID', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'UPRICE', type: 'string' },
            { name: 'CR_STATUS', type: 'string' },
            { name: 'ORDERTIME', type: 'string' },
            { name: 'ORDERID', type: 'string' },
            { name: 'EMAILTIME', type: 'string' },
            { name: 'REPLYTIME', type: 'string' },
            { name: 'INQTY', type: 'string' },
            { name: 'LOT_NO', type: 'string' },
            { name: 'EXP_DATE', type: 'string' },
            { name: 'ACKMMCODE', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'BACKQTY', type: 'string' },
            { name: 'BACKTIME', type: 'string' },
            { name: 'BACKID', type: 'string' },

            { name: 'ISCFM', type: 'string' },

            { name: 'CFMQTY', type: 'string' },
            { name: 'CFMTIME', type: 'string' },
            { name: 'CFMID', type: 'string' },
            { name: 'PURAPPTIME', type: 'string' },
            { name: 'PURACCTIME', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },

            { name: 'APPID_NAME', type: 'string' },
            { name: 'ORDERID_NAME', type: 'string' },
            { name: 'ACKID_NAME', type: 'string' },
            { name: 'BACKID_NAME', type: 'string' },
            { name: 'CFMID_NAME', type: 'string' },
            { name: 'CREATE_USER_NAME', type: 'string' },
            { name: 'UPDATE_USER_NAME', type: 'string' },

            { name: 'AGEN_COMBINE', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'AGEN_TEL', type: 'string' },
            { name: 'AGEN_BOSS', type: 'string' },

        ]
    });

    var windowHeight = $(window).height();
    var windowWidth = $(window).width();


    var T1Set = '';
    var T1LastRec = null;
    var maxExpDate = '';

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'CRDOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0111/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    crdocno: T1Query.getForm().findField('CRDOCNO').getValue(),
                    mmcode: T1Query.getForm().findField('MMCODE').getValue(),
                    agen_no: T1Query.getForm().findField('AGEN_NO').getValue(),
                    start_date: T1Query.getForm().findField('START_DATE').rawValue,
                    end_date: T1Query.getForm().findField('END_DATE').rawValue,
                    status: T1Query.getForm().findField('STATUS').getValue() == null ? '' : T1Query.getForm().findField('STATUS').getValue(),
                    isAA: isAA
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'LOT_NO', direction: 'ASC' }, { property: 'EXP_DATE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0111/Details',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    crdocno: T1LastRec.data.CRDOCNO
                };
                Ext.apply(store.proxy.extraParams, np);
            },
        }
    });

    var agenStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var statusStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setCombo() {
        Ext.Ajax.request({
            url: '/api/AB0111/GetAgenCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts;
                    for (var i = 0; i < temp.length; i++) {
                        agenStore.add(temp[i]);
                    }

                } 
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });

        Ext.Ajax.request({
            url: '/api/AB0111/GetStatusCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts;
                    for (var i = 0; i < temp.length; i++) {
                        statusStore.add(temp[i]);
                    }

                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }
    setCombo();


    function T1Load() {
        T1LastRec = null;
        T1Tool.moveFirst();
        msglabel('');

        T2Store.removeAll();
    }
    function T2Load() {
        T2Tool.moveFirst();
        msglabel('');
    }

    // 查詢欄位
    var mLabelWidth = 130;
    var mWidth = 310;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '緊急醫療出貨單編號',
                                name: 'CRDOCNO',
                            },
                            {
                                xtype: 'textfield',
                                fieldLabel: '申請院內碼',
                                name: 'MMCODE',
                            },
                            {
                                xtype: 'combo',
                                store: agenStore,
                                fieldLabel: '廠商',
                                name: 'AGEN_NO',
                                queryMode: 'local',
                                anyMatch: true,
                                autoSelect: true,
                                multiSelect: false,
                                displayField: 'TEXT',
                                valueField: 'VALUE',
                                requiredFields: ['TEXT'],
                                labelWidth: 70,
                                width: 200,
                                allowBlank: true, 
                                tpl: new Ext.XTemplate(
                                    '<tpl for=".">',
                                    '<tpl if="VALUE==\'\'">',
                                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                                    '<tpl else>',
                                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                                    '</tpl></tpl>', {
                                        formatText: function (text) {
                                            return Ext.util.Format.htmlEncode(text);
                                        }
                                    }),
                                listeners: {
                                    beforequery: function (record) {
                                        record.query = new RegExp(record.query, 'i');
                                        record.forceAll = true;
                                    },
                                    
                                },
                                padding: '0 4 0 4'
                            },
                        ]
                    },
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        bodyStyle: 'padding: 3px 5px;',
                        items: [
                            {
                                xtype: 'datefield',
                                fieldLabel: '要求到貨日期',
                                name: 'START_DATE',
                                width: 210,
                                padding: '0 4 0 4',
                            },
                            {
                                xtype: 'datefield',
                                fieldLabel: '至',
                                name: 'END_DATE',
                                // enforceMaxLength: true,
                                //maxLength: 5,
                                //minLength: 5,
                                //regexText: '請填入民國年月',
                                //regex: /\d{5,5}/,
                                labelWidth: 8,
                                labelSeperator: '',
                                width: 88,
                                padding: '0 4 0 4',
                            },
                            {
                                xtype: 'combo',
                                store: statusStore,
                                fieldLabel: '狀態',
                                name: 'STATUS',
                                queryMode: 'local',
                                anyMatch: true,
                                autoSelect: true,
                                multiSelect: true,
                                displayField: 'TEXT',
                                valueField: 'VALUE',
                                requiredFields: ['TEXT'],
                                labelWidth: 70,
                                width: 200,
                                fieldCls: '',
                                allowBlank: true, // 欄位為必填
                                tpl: new Ext.XTemplate(
                                    '<tpl for=".">',
                                    '<tpl if="VALUE==\'\'">',
                                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                                    '<tpl else>',
                                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                                    '</tpl></tpl>', {
                                        formatText: function (text) {
                                            return Ext.util.Format.htmlEncode(text);
                                        }
                                    }),
                                listeners: {
                                    beforequery: function (record) {
                                        record.query = new RegExp(record.query, 'i');
                                        record.forceAll = true;
                                    },

                                },
                                padding: '0 4 0 4'
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    T1Load();
                                }
                            }, {
                                xtype: 'button',
                                text: '清除',
                                handler: function () {
                                    var f = this.up('form').getForm();
                                    f.reset();
                                    msglabel('');
                                }
                            }
                        ]
                    }
                ]
            },
        ]
    });
    

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print', text: '列印出貨三聯單', 
                id: 'print', disabled: true,
                handler: function () {
                    if (T1LastRec == null) {
                        Ext.Msg.alert('提示', '請選擇緊急醫療出貨單');
                        return;
                    }
                    var cr_status = T1LastRec.data.CR_STATUS.substring(0, 1);

                    if (cr_status == 'A'
                        || cr_status == 'C'
                        || cr_status == 'D'
                        || cr_status == 'J'
                        || cr_status == 'P') {
                        Ext.Msg.alert('提示', '申請單狀態：' + T1LastRec.data.CR_STATUS + '，不可列印');
                        return;
                    }
                    showReport(T1LastRec.data.CRDOCNO);
                }
            },
            {
                itemId: 'changePtName', text: '修改病人資料', disabled: true,
                id: 'changePtName',
                handler: function () {
                    ptNameWindow.show();
                }
            },
            {
                itemId: 'export', text: '匯出',
                handler: function () {
                    var p = new Array();

                    /*
                     crdocno: T1Query.getForm().findField('CRDOCNO').getValue(),
                    mmcode: T1Query.getForm().findField('MMCODE').getValue(),
                    agen_no: T1Query.getForm().findField('AGEN_NO').getValue(),
                    start_date: T1Query.getForm().findField('START_DATE').rawValue,
                    end_date: T1Query.getForm().findField('END_DATE').rawValue,
                    status: T1Query.getForm().findField('STATUS').getValue(),
                    isAA: isAA
                    */

                    p.push({ name: 'crdocno', value: T1Query.getForm().findField('CRDOCNO').getValue() });
                    p.push({ name: 'mmcode', value: T1Query.getForm().findField('MMCODE').getValue() });
                    p.push({ name: 'agen_no', value: T1Query.getForm().findField('AGEN_NO').getValue() });
                    p.push({ name: 'start_date', value: T1Query.getForm().findField('START_DATE').rawValue });
                    p.push({ name: 'end_date', value: T1Query.getForm().findField('END_DATE').rawValue });
                    p.push({ name: 'status', value: T1Query.getForm().findField('STATUS').getValue() == null ? '' : T1Query.getForm().findField('STATUS').getValue() });
                    p.push({ name: 'isAA', value: isAA });

                    PostForm('/api/AB0111/Excel', p);
                    msglabel('訊息區:匯出完成');

                }
            }
        ]
    });
    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "緊急醫療出貨單編號",
                dataIndex: 'CRDOCNO',
                width: 120,
                renderer: function (val, meta, record) {
                    var CRDOCNO = record.data.CRDOCNO;
                    return '<a href=javascript:void(0)>' + CRDOCNO + '</a>';
                },
            },
            {
                text: "申請院內碼",
                dataIndex: 'MMCODE',
                width: 110,
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150,
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 150,
            },
            {
                text: "申請數量",
                dataIndex: 'APPQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 80,
            },
            {
                text: "入庫庫房",
                dataIndex: 'WH_NAME',
                width: 150,
            },
            {
                text: "要求到貨日",
                dataIndex: 'REQDATE',
                width: 80,
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_COMBINE',
                width: 100,
            },
            {
                text: "進貨量",
                dataIndex: 'INQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "點收院內碼",
                dataIndex: 'ACKMMCODE',
                width: 80,
            },
            {
                text: "點收量",
                dataIndex: 'ACKQTY',
                width: 110,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "退回量",
                dataIndex: 'BACKQTY',
                width: 110,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "結驗量",
                dataIndex: 'CFMQTY',
                width: 110,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "狀態",
                dataIndex: 'CR_STATUS',
                width: 110,
            },
            {
                header: "",
                flex: 1
            }
        ]
        ,
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');

                T1LastRec = record;

                ptNameForm.loadRecord(T1LastRec);

                Ext.getCmp('changePtName').disable();
                if (T1LastRec != null) {
                    Ext.getCmp('changePtName').enable();
                }
                T2Load();

                var cr_status = T1LastRec.data.CR_STATUS.substring(0, 1);

                Ext.getCmp('print').enable();
                if (cr_status == 'A'
                    || cr_status == 'C'
                    || cr_status == 'D'
                    || cr_status == 'J'
                    || cr_status == 'P') {
                    Ext.getCmp('print').disable();
                }

            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CRDOCNO');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }

                // T61LastRec = record;
                T1cell = 'cell';
                //
                T1LastRec = Ext.clone(record);

                Ext.getCmp('changePtName').disable();
                if (T1LastRec != null) {
                    Ext.getCmp('changePtName').enable();
                }

                CrDocForm.loadRecord(T1LastRec);
                ptNameForm.loadRecord(T1LastRec);

                formWindow.show();

            },
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 110,
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 80,
            },
            {
                text: "點收量",
                dataIndex: 'INQTY',
                width: 80,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "刷UDI",
                dataIndex: 'ISUDI',
                width: 80
            },
            {
                header: "",
                flex: 1
            }
        ]
    });

    //#region formWindow
    var CrDocForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'displayfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                items:
                [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '緊急醫療出貨單編號',
                        name: 'CRDOCNO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請院內碼',
                        name: 'MMCODE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '狀態',
                        name: 'CR_STATUS',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        labelAlign: 'right',
                        colspan: 3,
                        width: mWidth * 3
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        labelAlign: 'right',
                        colspan: 3,
                        width: mWidth * 3
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請量',
                        name: 'APPQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '計量單位',
                        name: 'BASE_UNIT',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '入庫庫房名稱',
                        name: 'WH_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '要求到貨日期',
                        name: 'REQDATE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單價',
                        name: 'CR_UPRICE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '收費屬性',
                        name: 'M_PAYKIND',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '使用醫師',
                        name: 'DRNAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '病人姓名',
                        name: 'PATIENTNAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '病人病歷號',
                        name: 'CHARTNO',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請人',
                        name: 'APPID_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請時間',
                        name: 'APPTIME',
                        labelAlign: 'right',
                        colspan: 2
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商名稱',
                        name: 'AGEN_COMBINE',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商EMAIL',
                        name: 'EMAIL',
                        labelAlign: 'right',
                        colspan: 2
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '產生通知單時間',
                        name: 'ORDERTIME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '產生通知單人員',
                        name: 'ORDERID_NAME',
                        labelAlign: 'right',
                        colspan: 2
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '寄EMAIL時間',
                        name: 'EMAILTIME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '收信確認時間',
                        name: 'REPLYTIME',
                        labelAlign: 'right',
                        colspan: 2
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商回覆數量',
                        name: 'INQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商回覆批號',
                        name: 'LOT_NO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商回覆效期',
                        name: 'EXP_DATE',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收院內碼',
                        name: 'ACKMMCODE',
                        labelAlign: 'right',
                        colspan: 3
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收量',
                        name: 'ACKQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收人',
                        name: 'ACKID_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '點收時間',
                        name: 'ACKTIME',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '退回量',
                        name: 'BACKQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '退回人',
                        name: 'BACKID_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '退回時間',
                        name: 'BACKTIME',
                        labelAlign: 'right',
                        colspan: 1
                    },

                    {
                        xtype: 'displayfield',
                        fieldLabel: '結驗量',
                        name: 'CFMQTY',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '結驗人',
                        name: 'CFMID_NAME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '結驗時間',
                        name: 'CFMTIME',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '產生採購申請單時間',
                    //    name: 'PURAPPTIME',
                    //    labelAlign: 'right',
                    //    colspan: 1
                    //},
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '採購進貨接收時間',
                    //    name: 'PURACCTIME',
                    //    labelAlign: 'right',
                    //    colspan: 1
                    //},
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '進貨量',
                    //    name: 'INQTY',
                    //    labelAlign: 'right',
                    //    colspan: 1
                    //},
                    {
                        xtype: 'displayfield',
                        fieldLabel: '結驗批號效期',
                        name: 'DETAIL',
                        labelAlign: 'right',
                        colspan: 3,
                        width:'100%'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '小採單號',
                        name: 'SMALL_DN',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請單號',
                        name: 'RDOCNO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申購單號',
                        name: 'PR_NO',
                        labelAlign: 'right',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '訂單號',
                        name: 'PO_NO',
                        labelAlign: 'right',
                        colspan: 3,
                        width:'100%'
                    },
                ]
            }
        ]
    });

    var formWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [CrDocForm],
        width: windowWidth,
        height: windowHeight,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "緊急醫療出貨單",
        buttons: [
            
            {
                text: '關閉',
                handler: function () {
                    formWindow.hide();
                }
            }
        ]
    });
    formWindow.hide();
    //#endregion

    //#region ptNameWindow
    var mmcodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'ACKMMCODE',
        fieldLabel: '點收院內碼',
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0110/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                mmcodeForm.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                mmcodeForm.getForm().findField('MMNAME_E').setValue(r.get('MMNAME_E'));
                mmcodeForm.getForm().findField('BASE_UNIT').setValue(r.get('BASE_UNIT'));
            }
        }
    });

    var ptNameForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: 200
        },
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '緊急醫療出貨單編號',
                name: 'CRDOCNO',
            },
            {
                xtype: 'textfield',
                fieldLabel: '使用醫師',
                name: 'DRNAME',
                maxLength: 20,
                enforceMaxLength: true
            },
            {
                xtype: 'textfield',
                fieldLabel: '病人姓名',
                name: 'PATIENTNAME',
                maxLength: 20,
                enforceMaxLength: true
            },
            {
                xtype: 'textfield',
                fieldLabel: '病人病歷號',
                name: 'CHARTNO',
                maxLength: 10,
                enforceMaxLength: true
            },
        ]
    });

    function changePtName() {
        var myMask = new Ext.LoadMask(ptNameWindow, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0111/ChangePtName',
            method: reqVal_p,
            params: {
                crdocno: ptNameForm.getForm().findField('CRDOCNO').getValue(),
                drName: ptNameForm.getForm().findField('DRNAME').getValue(),
                ptName: ptNameForm.getForm().findField('PATIENTNAME').getValue(),
                chartNo: ptNameForm.getForm().findField('CHARTNO').getValue(),
            },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('CRDOCNO').setValue(ptNameForm.getForm().findField('CRDOCNO').getValue());
                    ptNameWindow.hide();
                    msglabel('病人資料修改成功');

                    T1Load();
                   
                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var ptNameWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [ptNameForm],
        width: 350,
      //  height: 300,
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "修改病人資料",
        buttons: [
            {
                text: '儲存',
                handler: function () {
                    changePtName();

                    //if (!mmcodeForm.getForm().findField('ACKMMCODE').getValue()) {
                    //    Ext.Msg.alert('提醒', '');
                    //    return;
                    //}

                    //updateAckmmcode(CrDocForm.getForm().findField('CRDOCNO').getValue(), mmcodeForm.getForm().findField('ACKMMCODE').getValue());


                    //checkSetRatioEmpty();
                    //mmcodeWindow.hide();
                    //insertMulti();
                    //scanWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    ptNameWindow.hide();
                }
            }
        ]
    });
    ptNameWindow.hide();
    //#endregion

    function showReport(crdocno) {
        if (!win) {
            var np = {
                crdocno: crdocno
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?crdocno=' + np.crdocno + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    function confirm(crdocno) {

        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Confirm',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("點收成功");

                    confirmWindow.show();

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);

                    T1Query.getForm().findField('CRDOCNO').setValue(crdocno);
                    MasterLoad(crdocno);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function reject(crdocno) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Reject',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("退回成功");

                    T1Query.getForm().findField('CRDOCNO').setValue('');

                    var f = CrDocForm.getForm();
                    f.findField('CRDOCNO').setValue('');
                    f.findField('ACKMMCODE').setValue('');
                    f.findField('MMNAME_C').setValue('');
                    f.findField('MMNAME_E').setValue('');
                    f.findField('APPQTY').setValue('');
                    f.findField('BASE_UNIT').setValue('');
                    f.findField('INQTY').setValue('');
                    f.findField('WH_NAME').setValue('');
                    f.findField('CR_STATUS_NAME').setValue('');
                    f.findField('CR_STATUS').setValue('');
                    f.findField('LOT_NO').setValue('');
                    f.findField('EXP_DATE').setValue('');

                    Ext.getCmp('scanInsert').disable();
                    Ext.getCmp('insert').disable();
                    Ext.getCmp('importData').disable();
                    Ext.getCmp('update').disable();
                    Ext.getCmp('delete').disable();

                    T1Store.removeAll();

                    T1Query.getForm().findField('CRDOCNO').focus();

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);

                    T1Query.getForm().findField('CRDOCNO').setValue(crdocno);
                    MasterLoad(crdocno);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function importData(crdocno) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AB0110/Import',
            method: reqVal_p,
            params: {
                crdocno: crdocno
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    viewport.down('#form').collapse();

                    msglabel("載入成功");

                    MasterLoad(crdocno);

                } else {
                    myMask.hide();
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '60%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '40%',
                        split: true,
                        //items: [TATabs]
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        formWindow.setHeight(windowHeight);
        formWindow.setWidth(windowWidth);

        formWindow.center();
      
    });

    T1Query.getForm().findField('CRDOCNO').focus();

    Ext.getCmp('changePtName').hide();
    if (isAA == 'Y') {
        Ext.getCmp('changePtName').show();
    }
});