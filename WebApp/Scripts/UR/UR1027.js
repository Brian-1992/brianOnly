Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.File']);

Ext.onReady(function () {
    Ext.ariaWarn = Ext.emptyFn;
    var T1Get = '/api/UR1027/All';
    var T11Get = '/api/UR1027/DialogAll';
    var T1SendMsg = '/api/UR1027/SendMsg';
    //var T1Name = "個人訊息管理";
    var T1Name = '';

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'RECEIVE_USER', type: 'string' },
            { name: 'UR_MSG_SEQ', type: 'string' },
            { name: 'MSG_CONTENT', type: 'string' },
            { name: 'MSG_DATE', type: 'date' },
            { name: 'SEND_USER', type: 'string' },
            { name: 'READ_FLAG', type: 'string' },
            { name: 'ALERT_FLAG', type: 'string' }
        ]
    });

    var st_send_user = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/UR1027/GetSendUserCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        title: '',
        resizable: true,
        autoScroll: false,
        height: 65,
        bodyPadding: '0 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelT1P1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '發訊人',
                        name: 'P0',
                        id: 'P0',
                        enforceMaxLength: true,
                        maxLength: 20,
                        width: 250,
                        matchFieldWidth: false,
                        listConfig: { width: 170 },
                        labelWidth: 50,
                        store: st_send_user,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        anyMatch: true,
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '訊息內容',
                        name: 'P1',
                        id: 'P1',
                        enforceMaxLength: true,
                        maxLength: 350,
                        labelWidth: 70,
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '訊息日期',
                        name: 'P2',
                        id: 'P2',
                        width: 155,
                        vtype: 'dateRange',
                        dateRange: { end: 'P3' },
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P3',
                        id: 'P3',
                        width: 110,
                        labelWidth: 20,
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'P2' },
                        padding: '0 4 0 4',
                        value: new Date()
                    }
                ]
            }, {
                xtype: 'panel',
                id: 'PanelT1P2',
                border: false,
                layout: 'hbox',
                padding: '4 4 0 4',
                items: [
                    {
                        xtype: 'checkbox',
                        name: 'P4',
                        id: 'P4',
                        width: 150,
                        boxLabel: '包含自己發送的訊息',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 8'
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        handler: T1Load
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus();
                        }
                    }
                ]
            }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        autoLoad: true,
        sorters: [{ property: 'MSG_DATE', direction: 'DESC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').rawValue,
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST', // by default GET
                update: 'POST',
                destroy: 'POST'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T1Load() {
        T1Store.loadPage(1);
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'sendMsg', text: '發送訊息',
                handler: function () {
                    popSendMsgFormWin();
                }
            }
        ]
    });

    //懸浮提示初始化
    Ext.QuickTips.init();
    //屬性
    Ext.apply(Ext.QuickTips.getQuickTip(), {
        maxWidth: 700,
        dismissDelay: 60000,
        trackMouse: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query],
            resizable: true
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [
            { xtype: 'rownumberer' },
            {
                text: "發訊人",
                dataIndex: 'SEND_USER_NAME',
                width: 100,
                draggable: false,
                renderer: function (value, metaData, record, rowIndex) {
                    if (record.data.SEND_USER_NAME == '自己') {
                        return '<span style="color:gray;">' + value + '</span>';
                    } else {
                        return '<a href="javascript:popDialogFormWin(\'' + record.data.SEND_USER_NAME + '\',\'' + record.data.SEND_USER + '\')" >' + value + '</a>';
                    }
                }
            }, {
                text: "訊息內容",
                dataIndex: 'MSG_CONTENT',
                width: 550,
                draggable: false,
                renderer: function (value, metaData, record, rowIndex) {
                    if (value) {
                        value = Ext.String.htmlEncode(value);
                        metaData.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value.replace(/\n/g, '<br/>')) + '"';
                    }
                    if (record.data.SEND_USER_NAME == '自己') {
                        return '<span style="color:blue;">[to ' + record.data.RECEIVE_USER + ']</span> ' + value;
                    } else {
                        return value;
                    }
                }
            },
            {
                xtype: 'datecolumn',
                text: "訊息時間",
                dataIndex: 'MSG_DATE',
                format: 'X/m/d H:i',
                width: 150,
                draggable: false
            }],
        listeners: {
            cellclick: function (view, cell, cellIndex, record, row, rowIndex, e) {
                if (cellIndex == 2) {
                    Ext.Msg.alert(record.data['SEND_USER_NAME'], record.data['MSG_CONTENT']);
                }
            }
        }
    });

    // 發送訊息視窗
    var SendMsgFormWin = null;
    popSendMsgFormWin = function () {
        var st_receiveUser = Ext.create('Ext.data.Store', {
            proxy: {
                type: 'ajax',
                actionMethods: {
                    read: 'POST' // by default GET
                },
                url: '/api/UR1027/GetReceiveUserCombo',
                reader: {
                    type: 'json',
                    rootProperty: 'etts'
                }
            },
            listeners: {
                beforeload: function (store, options) {
                    // 載入前將查詢條件代入參數
                    var np = {
                        p0: SendMsgFormContent.getForm().findField('HistoryUser').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            }
        });

        var SendMsgFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 80,
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelSendMsgForm',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '收訊人',
                        name: 'ReceiveUser',
                        width: '100%',
                        store: st_receiveUser,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        forceSelection: true, anyMatch: true,
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        multiSelect: true,
                        delimiter: ',',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'checkbox',
                        name: 'HistoryUser',
                        width: '100%',
                        boxLabel: '只列出發送過的對象',
                        inputValue: 'Y',
                        checked: false,
                        padding: '0 4 0 90',
                        listeners:
                        {
                            change: function (rg, nVal, oVal, eOpts) {
                                SendMsgFormContent.getForm().findField('ReceiveUser').setValue('');
                                st_receiveUser.load();
                            }
                        }

                    }, {
                        xtype: 'textfield',
                        fieldLabel: '訊息內容',
                        name: 'MsgContent',
                        padding: '0 4 0 4',
                        width: '100%',
                        readOnly: false,
                        fieldCls: 'required',
                        allowBlank: false,
                        enforceMaxLength: true,
                        maxLength: 300
                    }
                ]
            }]
        });

        var SendMsgForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [SendMsgFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '發送',
                handler: function () {
                    var P0 = SendMsgFormContent.getForm().findField('ReceiveUser').getValue();
                    var P1 = SendMsgFormContent.getForm().findField('MsgContent').getValue();
                    if (SendMsgFormContent.getForm().isValid()) {
                        Ext.MessageBox.confirm('訊息', '是否確定傳送訊息?<br>' + P1, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: T1SendMsg,
                                    method: reqVal_p,
                                    params: {
                                        p0: P0,
                                        p1: P1
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            SendMsgFormWin.destroy();
                                            SendMsgFormWin = null;
                                            T1Load();
                                        } else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                        }
                                    },
                                    failure: function (response, options) {
                                        Ext.MessageBox.alert('錯誤', '傳送發生錯誤');
                                    }
                                });
                            }
                        }
                        );
                    }
                    else 
                        Ext.MessageBox.alert('提示', '請選擇收訊人並填寫訊息內容');
                }
            }, {
                disabled: false,
                text: '關閉',
                handler: function () {
                    SendMsgFormWin.destroy();
                    SendMsgFormWin = null;
                }
            }]
        });
        SendMsgFormWin = GetPopWin(viewport, SendMsgForm, '發送訊息', 600, 200);

        SendMsgFormWin.show();

        st_receiveUser.load();
    }

    // 對話視窗
    var DialogFormWin = null;
    popDialogFormWin = function (receiveUserName, receiveUser) {
        var T11Store = Ext.create('Ext.data.Store', {
            model: 'T1Model',
            pageSize: 100,
            remoteSort: true,
            autoLoad: true,
            sorters: [{ property: 'MSG_DATE', direction: 'DESC' }],

            listeners: {
                beforeload: function (store, options) {
                    var np = {
                        p0: receiveUser
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
            },
            proxy: {
                type: 'ajax',
                actionMethods: {
                    create: 'POST',
                    read: 'POST', // by default GET
                    update: 'POST',
                    destroy: 'POST'
                },
                url: T11Get,
                reader: {
                    type: 'json',
                    rootProperty: 'etts',
                    totalProperty: 'rc'
                }
            }
        });
        function T11Load() {
            T11Store.loadPage(1);
        }

        var T11Grid = Ext.create('Ext.grid.Panel', {
            store: T11Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            width: 750,
            height: 380,
            columns: [
                {
                    text: "發訊人",
                    dataIndex: 'SEND_USER_NAME',
                    width: 100,
                    draggable: false,
                    sortable: false,
                    renderer: function (value, metaData, record, rowIndex) {
                        if (record.data.SEND_USER_NAME == '自己') {
                            return '<span style="color:gray;">' + value + '</span>';
                        } else {
                            return value;
                        }
                    }
                }, {
                    text: "訊息內容",
                    dataIndex: 'MSG_CONTENT',
                    width: 400,
                    draggable: false,
                    sortable: false,
                    renderer: function (value, metaData, record, rowIndex) {
                        if (value) {
                            value = Ext.String.htmlEncode(value);
                            metaData.tdAttr = 'data-qtip="' + Ext.String.htmlEncode(value.replace(/\n/g, '<br/>')) + '"';
                        }
                        if (record.data.SEND_USER_NAME == '自己') {
                            return '<span style="color:blue;">' + value + '</span>';
                        } else {
                            return value;
                        }
                    }
                },
                {
                    xtype: 'datecolumn',
                    text: "訊息時間",
                    dataIndex: 'MSG_DATE',
                    format: 'X/m/d H:i',
                    width: 150,
                    draggable: false
                }, {
                    text: "",
                    dataIndex: 'READ_FLAG',
                    width: 60,
                    draggable: false,
                    sortable: false,
                    flex: 1,
                    renderer: function (value, metaData, record, rowIndex) {
                        if (record.data.SEND_USER_NAME == '自己' && value == 'Y') {
                            return '<span style="color:green;">已讀</span>';
                        }
                    }
                }
            ]
        });

        var DialogFormContent = Ext.widget({
            xtype: 'form',
            layout: 'form',
            title: '',
            bodyPadding: '4 0 4 0', //top right bottom left
            bodyStyle: ' ',
            fieldDefaults: {
                labelWidth: 80,
                labelAlign: 'right'
            },
            items: [
                T11Grid, 
                {
                xtype: 'panel',
                id: 'PanelDialogForm',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'textfield',
                        fieldLabel: '內容',
                        name: 'MsgContent',
                        padding: '0 4 0 4',
                        width: 650,
                        labelWidth: 40,
                        readOnly: false,
                        enforceMaxLength: true,
                        maxLength: 300
                    }, {
                        xtype: 'button',
                        text: '發送',
                        handler: function () {
                            var sendMsgContent = DialogFormContent.getForm().findField('MsgContent').getValue();
                            if (sendMsgContent) {
                                Ext.Ajax.request({
                                    url: T1SendMsg,
                                    method: reqVal_p,
                                    params: {
                                        p0: receiveUser,
                                        p1: sendMsgContent
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T11Load();
                                        } else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                        }
                                    },
                                    failure: function (response, options) {
                                        Ext.MessageBox.alert('錯誤', '傳送發生錯誤');
                                    }
                                });
                            }
                        }
                    }
                ]
            }]
        });

        var DialogForm = Ext.create('Ext.form.Panel', {
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [DialogFormContent],
            buttonAlign: 'center',
            buttons: [{
                disabled: false,
                text: '關閉',
                handler: function () {
                    clearInterval(intervalID);
                    DialogFormWin.destroy();
                    DialogFormWin = null;
                }
            }]
        });
        DialogFormWin = GetPopWin(viewport, DialogForm, receiveUserName, 800, 600);

        DialogFormWin.show();

        T11Load();

        //每分鐘更新一次對話內容
        var intervalID = setInterval(function () {
            T11Load();
        }, 60000);
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
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        items: [T1Grid]
                    }
                ]
            }]
        }
        ]
    });

    //T1Query.getForm().findField('P0').focus();
});
