Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1LastRec = null, T2LastRec = null;

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CRDOCNO', type: 'string' },
            { name: 'ACKMMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'INID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'AGEN_COMBINE', type: 'string' },
            { name: 'TEL', type: 'string' },
            { name: 'USEWHEN', type: 'string' },
            { name: 'USEWHERE', type: 'string' },
            { name: 'MEMO_DETAIL', type: 'string' },
            { name: 'M_PAYKIND', type: 'string' },
            { name: 'CR_UPRICE', type: 'string' },
            { name: 'CFMQTY', type: 'string' },
            { name: 'CR_STATUS', type: 'string' },
            { name: 'CR_STATUS_NAME', type: 'string' },
            { name: 'CFMSCAN', type: 'string' },
            { name: 'CFMSCAN_NAME', type: 'string' },
        ]
    });

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
            url: '/api/AA0144/GetCrDocsScan',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                
            },
        }
    });
    function T1Load() {

        T1LastRec = null;
        Ext.getCmp('btnRemove').disable();
        T1Tool.moveFirst();
    }

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'CRDOCNO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0144/GetCrDocsList',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc' //筆數
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    crdocno: T2Query.getForm().findField('CRDOCNO').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
                
            },
        }
    });
    function T2Load() {
        T2LastRec = null;
        Ext.getCmp('btnCfmQtyChange').disable();
        T2Tool.moveFirst();
    }
   
    // 查詢欄位
    var mLabelWidth = 120;
    var mWidth = 300;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 130,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            name: 'CRDOCNO',
                            fieldLabel: '刷緊急醫療出貨單編號',
                            allowBlank: true,
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length > 7) {
                                        scan();
                                    }
                                }
                            }
                        },{
                            xtype: 'button',
                            text: '顯示',
                            handler: function () {
                                msglabel('訊息區:');
                                scan();
                            }
                        }
                    ]
                },
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '結驗',
                id: 'btnCfm',
                name: 'btnCfm',
                handler: function () {
                    var tempData = T1Grid.getStore().data.items;

                    Ext.MessageBox.confirm('', '共將 <span style="color:red; font-weight:bold">' + tempData.length + '</span> 筆緊急醫療出貨資料結驗<br/>是否確定？', function (btn, text) {
                        if (btn === 'yes') {
                            var data = [];

                            for (var i = 0; i < tempData.length; i++) {
                                data.push(tempData[i].data);
                            }

                            confirm(data);
                        }
                    }
                    );
                }
            },
            {
                text: '退回廠商',
                id: 'btnReject',
                name: 'btnReject',
                handler: function () {
                    var tempData = T1Grid.getStore().data.items;

                    Ext.MessageBox.confirm('', '共將 <span style="color:red; font-weight:bold">' + tempData.length +'</span> 筆緊急醫療出貨資料退回廠商<br/>是否確定？', function (btn, text) {
                        if (btn === 'yes') {
                            var data = [];

                            for (var i = 0; i < tempData.length; i++) {
                                data.push(tempData[i].data);
                            }

                            reject(data);
                        }
                    }
                    );
                }
            },
            {
                text: '刪除',
                id: 'btnRemove',
                name: 'btnRemove',
                disabled: true,
                handler: function () {
                    remove();
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
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
                width: 130
            },
            {
                text: "院內碼",
                dataIndex: 'ACKMMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 80
            },
            {
                text: "點收量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'ACKQTY',
                width: 90
            },
            {
                text: "結驗量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'CFMQTY',
                width: 90
            },
            {
                text: "計量單位",
                dataIndex: 'BASEUNIT',
                width: 75
            },
            {
                text: "入庫庫房名稱",
                dataIndex: 'WH_NAME',
                width: 90
            },
            {
                text: "點收人員",
                dataIndex: 'ACKID',
                width: 90
            },
            {
                text: "點收時間",
                dataIndex: 'ACKTIME',
                width: 90
            },
            {
                text: "供應商名稱",
                dataIndex: 'AGEN_COMBINE',
                width: 90
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                T1LastRec = record;

                if (T1LastRec != null) {
                    Ext.getCmp('btnRemove').enable();
                }

                //if (T1LastRec.data["STATUS"] != 'P:完成過帳') {
                //    Ext.getCmp('btnCheck').setDisabled(true);
                //}
                //else {
                //    Ext.getCmp('btnCheck').setDisabled(false);
                //}
            }
        }
    });

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 120,
            width: 300
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            name: 'CRDOCNO',
                            fieldLabel: '緊急醫療出貨單編號',
                            allowBlank: true,
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                T2Load();
                            }
                        }
                        , {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                            }
                        }
                    ]
                },
            ]
        }]
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        buttons: [
            {
                text: '修改結驗量',
                id: 'btnCfmQtyChange',
                name: 'btnCfmQtyChange',
                disabled: true,
                handler: function () {
                    cfmWindow.show();
                    msglabel('');
                }
            }
        ]
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        title: '已點收區',
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
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
                text: "緊急醫療出貨單編號",
                dataIndex: 'CRDOCNO',
                width: 130
            },
            {
                text: "院內碼",
                dataIndex: 'ACKMMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 80
            },
            {
                text: "點收量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'ACKQTY',
                width: 90
            },
            {
                text: "結驗量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'CFMQTY',
                width: 90
            },
            {
                text: "計量單位",
                dataIndex: 'BASEUNIT',
                width: 75
            },
            {
                text: "入庫庫房名稱",
                dataIndex: 'WH_NAME',
                width: 90
            },
            {
                text: "點收人員",
                dataIndex: 'ACKID',
                width: 90
            },
            {
                text: "點收時間",
                dataIndex: 'ACKTIME',
                width: 90
            },
            {
                text: "供應商名稱",
                dataIndex: 'AGEN_COMBINE',
                width: 90
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');

                T2LastRec = record;

                cfmForm.loadRecord(T2LastRec);

                Ext.getCmp('btnCfmQtyChange').disable();
                if (T2LastRec != null) {
                    Ext.getCmp('btnCfmQtyChange').enable();
                }
            },
        }
    });

    function clearCfmScan() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0144/ClearCfmScan',
            method: reqVal_p,
            params: {},
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {

                    T2Load();

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function scan() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0144/Scan',
            method: reqVal_p,
            params: {crdocno: T1Query.getForm().findField('CRDOCNO').getValue()},
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Query.getForm().findField('CRDOCNO').setValue('');

                    T1Load();
                    T2Load();

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                    
                    T1Query.getForm().findField('CRDOCNO').setValue('');
                    T1Query.getForm().findField('CRDOCNO').focus();
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function remove() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        
        Ext.Ajax.request({
            url: '/api/AA0144/Remove',
            method: reqVal_p,
            params: { crdocno: T1LastRec.data.CRDOCNO },
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    
                    T1Load();
                    T2Load();

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function reject(list) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AA0144/Reject',
            method: reqVal_p,
            contentType: "application/json",
            params: {
                list: Ext.util.JSON.encode(list)
            },
            success: function (response) {

                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('退回廠商成功');
                    T1Load();
                    T2Load();

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },

            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    function confirm(list) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AA0144/Confirm',
            method: reqVal_p,
            contentType: "application/json",
            params: {
                list: Ext.util.JSON.encode(list)
            },
            success: function (response) {

                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('結驗成功');
                    T1Load();
                    T2Load();

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },

            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    //#region cfmWindow
    
    function updateCfmQty() {
        var myMask = new Ext.LoadMask(cfmWindow, { msg: '處理中...' });
        Ext.Ajax.request({
            url: '/api/AA0144/UpdateCfmQty',
            params: {
                crdocno: cfmForm.getForm().findField('CRDOCNO').getValue(),
                cfmQty: cfmForm.getForm().findField('CFMQTY').getValue(),
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('修改結驗量成功');
                } else {
                    msglabel('修改結驗量失敗');
                }
              //  T2Query.getForm().findField('CRDOCNO').setValue(cfmForm.getForm().findField('CRDOCNO').getValue());

                T2Load();
                cfmWindow.hide();

            },
            failure: function (response, options) {
                var data = Ext.decode(response.responseText);
                Ext.Msg.alert('訊息', '<span style=\'color:red\'>' + data.msg + '</span>');
            }
        });
    }

    var cfmForm = Ext.widget({
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
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'CRDOCNO',
                            fieldLabel: '緊急醫療出貨單編號',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'ACKMMCODE',
                            fieldLabel: '院內碼',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'MMNAME_C',
                            fieldLabel: '中文品名',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'MMNAME_E',
                            fieldLabel: '英文品名',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'ACKQTY',
                            fieldLabel: '點收量',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'BASE_UNIT',
                            fieldLabel: '計量單位',
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '結驗量',
                            name: 'CFMQTY',
                            enforceMaxLength: false,
                            maxLength: 14,
                            minValue: 1,
                            decimalPrecision:0,
                            hideTrigger: true
                        }
                    ]
                },
            ]
        }]
    });
    var cfmWindow = Ext.create('Ext.window.Window', {
        id: 'cfmForm',
        renderTo: Ext.getBody(),
        items: [cfmForm],
        modal: true,
        //width: 550,
        //height: 200,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        buttons: [
            {
                text: '儲存',
                handler: function () {
                    var f = cfmForm.getForm()
                    if (f.findField('CFMQTY').getValue() < 1) {
                        Ext.Msg.alert('提醒', '結驗量最小為1');
                        return;
                    }

                    if (Number(f.findField('ACKQTY').getValue()) < Number(f.findField('CFMQTY').getValue())) {
                        Ext.Msg.alert('提醒', '結驗量不可大於點收量');
                        return;
                    }

                    updateCfmQty();
                }
            },
            {
                text: '取消',
                handler: function () {
                    cfmWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                cfmWindow.center();
                //cfmWindow.setWidth(550);
            }
        }
    });
    //#endregion

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
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
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });

    clearCfmScan();
});