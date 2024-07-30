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
            { name: 'SET_YM', type: 'string' },
            { name: 'SET_BTIME', type: 'string' },
            { name: 'SET_ETIME', type: 'string' },
            { name: 'SET_STATUS', type: 'string' },
            { name: 'SET_MSG', type: 'string' },
            { name: 'SET_CTIME', type: 'string' }
        ]
    });
    var isFirst = true;
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
            fieldLabel: '月結年月',
            name: 'SET_YM',
            fieldCls: 'required',
            labelAlign: 'right',
            width: 200,
            readOnly: true
        }, {
            xtype: 'datefield',
            fieldLabel: '預計月結時間',
            name: 'P1',
            fieldCls: 'required',
            labelAlign: 'right',
            minValue: new Date(),
            //maxValue: Ext.Date.getLastDateOfMonth(new Date()),
            width: 200,
            allowBlank: false,
            blankText: '此欄位為必填'
        }, {
            xtype: 'button',
            text: '儲存',
            handler: T1Submit
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 15, 
        remoteSort: true,
        sorters: [{ property: 'SET_YM', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AA0130/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            load: function (store) {
                if (store.data.length >= 1 & isFirst==true) {
                    T1Form.getForm().findField('SET_YM').setValue(store.data.items[0].data.SET_YM);
                    T1Form.getForm().findField('P1').setValue(store.data.items[0].data.SET_CTIME);
                    isFirst = false;
                }
            }
        }
        , autoLoad: true
    });
    function T1Load() {
        T1Tool.moveFirst();
    }
    
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            f.findField('SET_CTIME').setValue(Ext.util.Format.date(f.findField('P1').getValue(), 'Ymd'));
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url:  '/api/AA0130/Update',
                success: function (form, action) {
                    myMask.hide();
                    msglabel('訊息區:資料修改成功');
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }

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
            text: "月結年月",
            dataIndex: 'SET_YM',
           // sortable: false,
            width: 80
        }, {
            text: "開帳時間",
            dataIndex: 'SET_BTIME',
            sortable: false,
            width: 100
        }, {
            text: "關帳時間",
            dataIndex: 'SET_ETIME',
            sortable: false,
            width: 100
        }, {
            text: "月結狀態",
            dataIndex: 'SET_STATUS',
            sortable: false,
            width: 120
        }, {
                text: "月結訊息",
             //   sortable: false,
            dataIndex: 'SET_MSG',
            flex:1
        }]   
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });
});
