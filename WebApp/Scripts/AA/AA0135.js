// 2020-05-27 新增補開盤點單功能
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

   
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'CHK_YM', type: 'string' },
            { name: 'SET_ATIME', type: 'string' },
            { name: 'SET_CTIME', type: 'string' }
        ]
    });
    var isFirst = true;
    var todayDateString = '';
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        autoScroll: true,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 100
        },
        border: false,
        items: [{
            name: 'SET_CTIME',
            xtype: 'hidden'
        }, {
            fieldLabel: '盤點年月',
            name: 'CHK_YM',
            fieldCls: 'required',
            labelAlign: 'right',
            width: 200,
            readOnly: true
        }, {
            xtype: 'datefield',
            fieldLabel: '預計開單日期',
            name: 'SET_CTIME',
            fieldCls: 'required',
            labelAlign: 'right',
            id:'SET_CTIME',
            minValue: new Date(),
            //maxValue: Ext.Date.getLastDateOfMonth(new Date()),
            width: 200,
            allowBlank: false,
            blankText: '此欄位為必填',
            value: currentLoad()
        }, {
            xtype: 'button',
            text: '儲存',
            handler: T1Submit
        }, {
                xtype: 'button',
                text: '補開本月盤點單',
                margin:'0 0 0 50 ',
                handler: function () {
                    whnoInputWindow.show();
                }
            }

        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, 
        remoteSort: true,
        sorters: [{ property: 'CHK_YM', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'GET'
            },
            url: '/api/AA0135/Done',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    function currentLoad() {
        Ext.Ajax.request({
            url: '/api/AA0135/Current',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Form.getForm().findField('CHK_YM').setValue(data.etts[0].CHK_YM);
                    Ext.getCmp('SET_CTIME').setValue(data.etts[0].SET_CTIME);
                    T1Load();
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }
    
    function T1Submit() {
        
        Ext.Ajax.request({
            url: '/api/AA0135/Update',
            method: reqVal_p,
            params: {
                chk_ym: T1Form.getForm().findField('CHK_YM').getValue(),
                set_ctime: Ext.getCmp('SET_CTIME').rawValue
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Form.getForm().findField('CHK_YM').setValue(data.etts[0].CHK_YM);
                    Ext.getCmp('SET_CTIME').setValue(data.etts[0].SET_CTIME);
                    msglabel('資料更新完成');
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }

    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0002/CurrentDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    todayDateString = data.msg;
                    whnoInputForm.getForm().findField('CHK_YM').setValue(todayDateString);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTodayDate();

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Form]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "盤點年月",
            dataIndex: 'CHK_YM',
           // sortable: false,
            width: 80
        }, {
            text: "預計開單日期",
            dataIndex: 'SET_CTIME',
            sortable: false,
            width: 100
        }, {
                text: "實際開單日期",
                dataIndex: 'SET_ATIME',
            sortable: false,
            width: 100
        }]   
    });

    var whnoInputForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name:'CHK_YM',
                            value: todayDateString
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '庫房代碼',
                            name: 'WH_NO'
                        }
                    ]
                },
            ]
        }]
    });
    var whnoInputWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [whnoInputForm],
        resizable: false,
        closable: false,
        title: "補開單庫房輸入",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    if (!whnoInputForm.getForm().findField('WH_NO').getValue()) {
                        Ext.Msg.alert('提醒', '請輸入庫房代碼');
                        return;
                    }
                    T2Store.removeAll();
                    Ext.Ajax.request({
                        url: '/api/AA0135/CreateChkMasts',
                        method: reqVal_p,
                        params: { wh_no: whnoInputForm.getForm().findField('WH_NO').getValue()},
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('盤點單新增完成');
                                
                                var results = data.etts;
                                if (results.length > 0) {
                                    for (var i = 0; i < results.length; i++) {
                                        T2Store.add(results[i]);
                                    }
                                }
                                whnoInputWindow.hide();
                                insertResultWindow.show();
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            hnoInputWindow.hide();
                        }
                    });

                    
                }
            },
            {
                text: '取消',
                handler: function () {
                    whnoInputForm.getForm().findField('WH_NO').setValue('');
                    whnoInputWindow.hide();
                }
            }
        ]
    });
    whnoInputWindow.hide();

    var T2Store = Ext.create('Ext.data.Store', {
        fields: ['CHK_TYPE', 'ITEM_STRING']
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        id: 'T2Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 120,
                renderer: function (val, meta, record) {
                    var chk_type = record.data.CHK_TYPE;
                    if (chk_type == '0') {
                        return '非庫備';
                    }
                    if (chk_type == '1') {
                        return '庫備';
                    }
                    if (chk_type == '3') {
                        return '小額採購';
                    }
                    return '';
                },
            },
            {
                text: "結果",
                dataIndex: 'ITEM_STRING',
                flex: 1
            }
        ],
    });
    var insertResultWindow = Ext.create('Ext.window.Window', {
        id: 'insertResultWindow',
        renderTo: Ext.getBody(),
        items: [
            T2Grid
        ],
        modal: true,
        width: "400px",
        resizable: false,
        draggable: true,
        closable: false,
        title: "開單結果",
        buttons: [ {
                text: '關閉',
                handler: function () {
                    insertResultWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                insertResultWindow.center();
            }
        }
    });
    insertResultWindow.hide();


    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
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
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    currentLoad();
});
