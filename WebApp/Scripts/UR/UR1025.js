Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.onReady(function () {
    var T1Name = '使用者權限明細';
    var T1Get = '/api/UR1025/All';
    var T11Get = '/api/UR1025/UserDetail';
    var T12Get = '/api/UR1025/FgDetail';
    var InidComboGet = '/api/BC0002/GetInidCombo';
    var T1GetExcel = '/api/UR1025/Excel';

    var T1Rec = 0;
    var T1LastRec = null;

    function T1Load() {
        msglabel('');
        T1Tool.moveFirst();
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'TUSER', type: 'string' },//帳號 UR_ID.TUSER
            { name: 'UNA', type: 'string' },//姓名 UR_ID.UNA
            { name: 'ADUSER', type: 'string' },//AD帳號 UR_ID.ADUSER
            { name: 'RLNO', type: 'string' },//群組代碼 UR_UIR.RLNO
            { name: 'RLNA', type: 'string' },//群組名稱 UR_ROLE.RLNA
            { name: 'RLDESC', type: 'string' },//群組說明 UR_ROLE.RLDESC
            { name: 'FG', type: 'string' },//程式編號 UR_TACL.FG
            { name: 'F1', type: 'string' },//程式名稱 UR_MENU.F1
            { name: 'R', type: 'string' },//可連結 UR_TACL.R
            { name: 'U', type: 'string' },//可修改 UR_TACL.U
            { name: 'P', type: 'string' },//可顯示 UR_TACL.P
            { name: 'INID', type: 'string' },//責任中心代碼 UR_ID.INID
            { name: 'INID_NAME', type: 'string' },//責任中心名稱 UR_ID.INID_NAME
            { name: 'IDDESC', type: 'string' }//人員描述 UR_ID.IDDESC
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        pageSize: 20,
        model: 'T1Model',
        //autoLoad: false,
        remoteSort: true,
        sorters: [{ property: 'TUSER', direction: 'ASC' }, { property: 'RLNO', direction: 'ASC' }, { property: 'FG', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('TUSER').getValue(),//帳號
                    p1: T1Query.getForm().findField('UNA').getValue(),//姓名
                    p2: T1Query.getForm().findField('FG').getValue(),//程式編號
                    p3: T1Query.getForm().findField('RLNO').getValue(),//群組代碼
                    p4: T1Query.getForm().findField('R').getValue(),//查詢
                    p5: T1Query.getForm().findField('U').getValue(),//維護
                    p6: T1Query.getForm().findField('P').getValue(),//列印
                    p7: T1Query.getForm().findField('INID').getValue(),//責任中心代碼
                    p8: T1Query.getForm().findField('ADUSER').getValue()//AD帳號
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                T1Grid.down('#t1export').setDisabled(records.length == 0);
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T1Get,
            timeout: 60000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    var cbInid = Ext.create('WEBAPP.form.UrInidCombo', {
        name: 'INID',
        fieldLabel: '責任中心',
        //queryParam: {
        //GRP_CODE: 'UR_INID',
        //DATA_NAME: 'INID_FLAG'
        //},
        //insertEmptyRow: true,
        limit: 20,
        queryUrl: InidComboGet,
        //forceSelection: true,
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        height: 65,
        autoScroll: true,
        border: false,
        items: [{
            xtype: 'container',
            layout: 'column',
            width: '90%',
            defaults: {
                layout: 'anchor',
                defaults: {
                    anchor: '100%',
                    labelAlign: "right",
                    labelWidth: 65,
                    margin: '5'
                }
            },
            items: [{
                xtype: 'container',
                // columnWidth: .2,
                width: 170,
                defaultType: 'textfield',
                items: [
                    {
                        fieldLabel: '帳號',
                        name: 'TUSER'
                    },
                    {
                        fieldLabel: '程式編號',
                        name: 'FG'
                    }]
            }, {
                xtype: 'container',
                // columnWidth: .25,
                width: 170,
                defaultType: 'textfield',
                items: [
                    {
                        fieldLabel: '姓名',
                        name: 'UNA'
                    },
                    {
                        fieldLabel: '群組代碼',
                        name: 'RLNO'
                    }]
            }, {
                xtype: 'container',
                //columnWidth: .25,
                width: 200,
                defaultType: 'textfield',
                items: [cbInid,
                {
                    fieldLabel: 'AD帳號',
                    name: 'ADUSER'
                }]
            },
            {
                xtype: 'container',
                //columnWidth: .3,
                width: 250,
                defaultType: 'textfield',
                items: [
                {
                    xtype: 'checkboxgroup',
                    items: [
                        { boxLabel: '查詢', name: 'R', checked: true },
                        { boxLabel: '維護', name: 'U', checked: true },
                        { boxLabel: '列印', name: 'P', checked: true }
                    ]
                    }, {
                        xtype: 'container',
                        defaultType: 'textfield',
                        items: [{
                            xtype: 'button',
                            text: '查詢',
                            handler: T1Load
                        }, {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('TUSER').focus();
                            }
                        }]
                }]
            }]
        }]

    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't1export', text: '匯出', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: T1Name + '.xlsx' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('TUSER').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('UNA').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('FG').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('RLNO').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('R').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('U').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P').getValue() });
                    p.push({ name: 'p7', value: T1Query.getForm().findField('INID').getValue() });
                    p.push({ name: 'p8', value: T1Query.getForm().findField('ADUSER').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',

        dockedItems: [{
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        },
        {
            text: "帳號",
            dataIndex: 'TUSER',
            width: 80,
            renderer: function (val, meta, record) {
                return '<a href=javascript:popUserDetail()>' + val + '</a>';
            }
        }, {
            text: "姓名",
            dataIndex: 'UNA',
            width: 100
        }, {
            text: "AD帳號",
            dataIndex: 'ADUSER',
            width: 100
        }, {
            text: "責任中心代碼",
            dataIndex: 'INID',
            width: 100
        }, {
            text: "群組代碼",
            dataIndex: 'RLNO',
            width: 80
        }, {
            text: "群組名稱",
            dataIndex: 'RLNA',
            width: 100
        }, {
            text: "群組說明",
            dataIndex: 'RLDESC',
            width: 100
        }, {
            text: "查詢",
            dataIndex: 'R',
            width: 50,
            renderer: function (val, meta, record) {
                if (val == '1')
                    return 'v';
                else
                    return '';
            }
        }, {
            text: "維護",
            dataIndex: 'U',
            width: 50,
            renderer: function (val, meta, record) {
                if (val == '1')
                    return 'v';
                else
                    return '';
            }
        }, {
            text: "列印",
            dataIndex: 'P',
            width: 50,
            renderer: function (val, meta, record) {
                if (val == '1')
                    return 'v';
                else
                    return '';
            }
        }, {
            text: "程式編號",
            dataIndex: 'FG',
            width: 100,
            renderer: function (val, meta, record) {
                return '<a href=javascript:popFgDetail()>' + val + '</a>';
            }
        }, {
            text: "程式名稱",
            dataIndex: 'F1',
            width: 170
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    var callableWin = null;
    // 帳號群組關聯
    popUserDetail = function () {
        if (!callableWin) {
            var T11Store = Ext.create('Ext.data.Store', {
                pageSize: 20,
                model: 'T1Model',
                //autoLoad: false,
                remoteSort: true,
                sorters: [{ property: 'RLNO', direction: 'ASC' }],
                listeners: {
                    beforeload: function (store, options) {
                        var np = {
                            p0: T1LastRec.data['TUSER']//帳號
                        };
                        Ext.apply(store.proxy.extraParams, np);
                    }
                },
                proxy: {
                    type: 'ajax',
                    actionMethods: {
                        read: 'POST' // by default GET
                    },
                    url: T11Get,
                    timeout: 60000,
                    reader: {
                        type: 'json',
                        rootProperty: 'etts',
                        totalProperty: 'rc'
                    }
                }
            });
            var T11Tool = Ext.create('Ext.PagingToolbar', {
                store: T11Store,
                displayInfo: true,
                border: false,
                plain: true
            });

            var popMainform = Ext.create('Ext.panel.Panel', {
                //id: 'userDetailCard',
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'grid',
                    id: 'userdetailform',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    store: T11Store,
                    plain: true,
                    loadingText: '處理中...',
                    loadMask: true,
                    cls: 'T1',
                    dockedItems: [{
                        dock: 'top',
                        xtype: 'toolbar',
                        items: [T11Tool]
                    }],
                    columns: [{ xtype: 'rownumberer' },
                    {
                        text: "群組代碼",
                        dataIndex: 'RLNO',
                        width: 80
                    }, {
                        text: "群組名稱",
                        dataIndex: 'RLNA',
                        width: 100
                    }, {
                        text: "群組說明",
                        dataIndex: 'RLDESC',
                        width: 120
                    }, {
                        header: "",
                        flex: 1
                    }
                    ]
                }],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        callableWin.destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popMainform, '帳號群組關聯(' + T1LastRec.data['TUSER'] + ')', viewport.width * 0.9, viewport.height * 0.9);

            T11Tool.moveFirst();
        }
        callableWin.show();
    }
    // 程式群組關聯
    popFgDetail = function () {
        if (!callableWin) {
            var T12Store = Ext.create('Ext.data.Store', {
                pageSize: 20,
                model: 'T1Model',
                //autoLoad: false,
                remoteSort: true,
                sorters: [{ property: 'RLNO', direction: 'ASC' }],
                listeners: {
                    beforeload: function (store, options) {
                        var np = {
                            p0: T1LastRec.data['FG']
                        };
                        Ext.apply(store.proxy.extraParams, np);
                    }
                },
                proxy: {
                    type: 'ajax',
                    actionMethods: {
                        read: 'POST' // by default GET
                    },
                    url: T12Get,
                    timeout: 60000,
                    reader: {
                        type: 'json',
                        rootProperty: 'etts',
                        totalProperty: 'rc'
                    }
                }
            });
            var T12Tool = Ext.create('Ext.PagingToolbar', {
                store: T12Store,
                displayInfo: true,
                border: false,
                plain: true
            });

            var popMainform = Ext.create('Ext.panel.Panel', {
                //id: 'userDetailCard',
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'grid',
                    id: 'fgdetailform',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    store: T12Store,
                    plain: true,
                    loadingText: '處理中...',
                    loadMask: true,
                    cls: 'T1',
                    dockedItems: [{
                        dock: 'top',
                        xtype: 'toolbar',
                        items: [T12Tool]
                    }],
                    columns: [{ xtype: 'rownumberer' },
                    {
                        text: "查詢",
                        dataIndex: 'R',
                        width: 50,
                        renderer: function (val, meta, record) {
                            if (val == '1')
                                return 'v';
                            else
                                return '';
                        }
                    }, {
                        text: "維護",
                        dataIndex: 'U',
                        width: 50,
                        renderer: function (val, meta, record) {
                            if (val == '1')
                                return 'v';
                            else
                                return '';
                        }
                    }, {
                        text: "列印",
                        dataIndex: 'P',
                        width: 50,
                        renderer: function (val, meta, record) {
                            if (val == '1')
                                return 'v';
                            else
                                return '';
                        }
                    }, {
                        text: "群組代碼",
                        dataIndex: 'RLNO',
                        width: 80
                    }, {
                        text: "群組名稱",
                        dataIndex: 'RLNA',
                        width: 100
                    }, {
                        text: "群組說明",
                        dataIndex: 'RLDESC',
                        width: 120
                    }, {
                        header: "",
                        flex: 1
                    }
                    ]
                }],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        callableWin.destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popMainform, '程式群組關聯(' + T1LastRec.data['FG'] + ')', viewport.width * 0.9, viewport.height * 0.9);

            T12Tool.moveFirst();
        }
        callableWin.show();
    }

    var viewport = Ext.create('Ext.Viewport', {
        id: "viewport",
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
            height: '50%',
            border: false,
            items: [T1Grid]
        }
        ]
    });

});
