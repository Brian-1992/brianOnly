Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Get = '/api/UR1022/Query';
    var T1Name = "";
    
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        autoScroll: true,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 60
        },
        border: false,
        items: [{
            fieldLabel: '姓名',
            name: 'P0',
            enforceMaxLength: true,
            maxLength: 20,
            labelWidth: 40,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }
        }, {
            xtype: 'datefield',
            fieldLabel: '登入日期',
            name: 'P1',
            enforceMaxLength: true,
            maxLength: 7,
            width: 150,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }
        }, {
            xtype: 'datefield',
            fieldLabel: '至',
            labelSeparator: '',
            labelWidth: 30,
            width: 120,
            name: 'P1_1',
            enforceMaxLength: true,
            maxLength: 7,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }
        }, {
            fieldLabel: '登入IP',
            name: 'P2',
            enforceMaxLength: true,
            maxLength: 20,
            padding: '0 4 0 4',
            listeners: {
                specialkey: function (field, e) {
                    if (e.getKey() === e.ENTER) {
                        T1Tool.moveFirst();
                    }
                }
            }
        }, {
            xtype: 'button',
            text: '查詢',
            handler: function () { T1Tool.moveFirst(); }
        }, {
            xtype: 'button',
            text: '清除',
            handler: function () {
                var f = T1Query.getForm();
                f.reset();
                f.findField('P0').focus();
            }
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'SID', type: 'string' },//錯誤號碼
            { name: 'TUSER', type: 'string' },//登入帳號
            { name: 'UNA', type: 'string' },//姓名
            { name: 'LOGIN_DATE', type: 'date', allowNull: true },//登入日期
            { name: 'LOGOUT_DATE', type: 'date', allowNull: true },//登出日期
            { name: 'USER_IP', type: 'string' }, //執行人員IP
            { name: 'AP_IP', type: 'string' } //AP IP
        ]
    });
    
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'LOGIN_DATE', direction: 'DESC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    UNA: T1Query.getForm().findField('P0').getValue(),
                    LOGIN_DATE_B: T1Query.getForm().findField('P1').getValue(),
                    LOGIN_DATE_E: T1Query.getForm().findField('P1_1').getValue(),
                    USER_IP: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T1Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
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
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        
        columns: [{ xtype: 'rownumberer' },
        {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 70
        }, {
                text: "姓名",
                dataIndex: 'UNA',
                width: 120
            }, {
            xtype: 'datecolumn',
            text: "登入日期",
            dataIndex: 'LOGIN_DATE',
            format: 'X/m/d H:i:s',
            width: 130
            }, {
                xtype: 'datecolumn',
                text: "登出日期",
                dataIndex: 'LOGOUT_DATE',
                format: 'X/m/d H:i:s',
                width: 130
            }, {
                text: "登入IP",
                dataIndex: 'USER_IP',
                width: 120
            }, {
                text: "AP IP",
                dataIndex: 'AP_IP',
                width: 120
            },
            { sortable: false, flex: 1 }
        ]
    });
    
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
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                items: [T1Grid]
            }]
        });

    T1Query.getForm().findField('P0').focus();
});
