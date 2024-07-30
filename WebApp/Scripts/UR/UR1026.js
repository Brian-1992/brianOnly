Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    //Master
    var T1Get = '/api/UR1026/QueryM';
    var T1GetExcel = '../../../api/UR1026/ExcelM';
    var T1Name = "";


    //Detail
    var T11Get = '/api/UR1026/QueryD';
    var T11GetExcel = '../../../api/UR1026/ExcelD';
    var T11Name = "參數明細";

    //Master
    var T1Rec = 0;
    var T1LastRec = null;
    var T1F1 = '';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: 60
        },
        border: false,
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                defaultType: 'textfield',
                items: [
                    {
                        fieldLabel: '記錄編號',
                        name: 'P0',
                        enforceMaxLength: true,
                        maxLength: 50,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Search();
                                }
                            }
                        }
                    }, {
                        fieldLabel: '姓名',
                        name: 'P1',
                        enforceMaxLength: true,
                        maxLength: 50,
                        labelWidth: 35,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Search();
                                }
                            }
                        }
                    }, {
                        fieldLabel: '程式代碼',
                        name: 'P2',
                        enforceMaxLength: true,
                        maxLength: 50,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Search();
                                }
                            }
                        }
                    }, {
                        fieldLabel: '函式代碼',
                        name: 'P5',
                        enforceMaxLength: true,
                        maxLength: 50,
                        padding: '0 4 0 4',
                        listeners: {
                            specialkey: function (field, e) {
                                if (e.getKey() === e.ENTER) {
                                    T1Search();
                                }
                            }
                        }
                    }

                ]
            }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                defaultType: 'textfield',
                items: [
                    {
                        xtype: 'datefield',
                        name: 'P3',
                        fieldLabel: '呼叫時間',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'datefield',
                        name: 'P4',
                        fieldLabel: '至',
                        labelWidth: 35,
                        labelSeparator: '',
                        padding: '0 4 0 4'
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            msglabel('');
                            T1Search();
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = T1Query.getForm();
                            f.reset();
                            f.findField('P0').focus();
                            msglabel('');
                        }
                    }
                ]
            }]
    });

    function T1Search() {
        T1Tool.moveFirst();
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'IDNO', type: 'int' },//記錄編號
            { name: 'TUSER', type: 'string' },//執行人員
            { name: 'UNA', type: 'string' },//執行人員姓名
            { name: 'IP', type: 'string' },//執行人員IP
            { name: 'CTRL', type: 'string' },//程式代碼(Controller Name)
            { name: 'ACT', type: 'string' },//函式代碼(Action Name)
            { name: 'CALL_TIME', type: 'date' }//呼叫時間
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20,
        remoteSort: true,
        sorters: [{ property: 'IDNO', direction: 'DESC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
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

    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1 },
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelAlign: 'right',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: '記錄編號',
            name: 'IDNO',
            readOnly: true,
            fieldCls: 'required'
        }, {
            fieldLabel: '帳號',
            name: 'TUSER',
            readOnly: true
        }, {
            fieldLabel: '姓名',
            name: 'UNA',
            readOnly: true
        }, {
            fieldLabel: 'IP',
            name: 'IP',
            readOnly: true
        }, {
            fieldLabel: '程式代碼',
            name: 'CTRL',
            readOnly: true
        }, {
            fieldLabel: '函式代碼',
            name: 'ACT',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '呼叫時間',
            name: 'CALL_TIME',
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'X/m/d H:i:s');
            }
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 't1export', text: '匯出',
            handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '使用者功能呼叫記錄.xls' });
                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });
                p.push({ name: 'p4', value: T1Query.getForm().findField('P4').rawValue });
                p.push({ name: 'p5', value: T1Query.getForm().findField('P5').rawValue });

                // 若所有查詢條件未設定,則先提示
                if (T1Query.getForm().findField('P0').getValue() == ''
                    && T1Query.getForm().findField('P1').getValue() == ''
                    && T1Query.getForm().findField('P2').getValue() == ''
                    && T1Query.getForm().findField('P5').getValue() == ''
                    && T1Query.getForm().findField('P3').rawValue == ''
                    && T1Query.getForm().findField('P4').rawValue == '') {
                    Ext.MessageBox.confirm('提示', '尚未設定查詢條件，匯出結果可能相當龐大，是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            PostForm(T1GetExcel, p);
                            msglabel('匯出完成');
                        }
                    });
                }
                else {
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            }
        }]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name,
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
            }, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],

        columns: [
            { xtype: 'rownumberer' },
            {
                text: "記錄編號",
                dataIndex: 'IDNO',
                width: 70
            }, {
                text: "帳號",
                dataIndex: 'TUSER',
                width: 70
            }, {
                text: "姓名",
                dataIndex: 'UNA',
                width: 100
            }, {
                text: "IP",
                dataIndex: 'IP',
                width: 120
            }, {
                text: "程式代碼",
                dataIndex: 'CTRL',
                width: 120
            }, {
                text: "函式代碼",
                dataIndex: 'ACT',
                width: 120
            }, {
                xtype: 'datecolumn',
                text: "呼叫時間",
                dataIndex: 'CALL_TIME',
                format: 'X/m/d H:i:s',
                width: 140
            }, 
            { sortable: false, flex: 1 }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                //viewport.down('#form').addCls('T1b');
            }
        }
    });
    function setFormT1a() {
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            T1F1 = f.findField('IDNO').getValue();
        }
        else {
            T1Form.getForm().reset();
            T1F1 = '';
        }
        T2Load();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'IDNO', type: 'int' },//記錄編號
            { name: 'PN', type: 'string' },//參數名稱
            { name: 'PV', type: 'string' } //參數值
        ]
    });


    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10,
        remoteSort: true,
        sorters: [{ property: 'PN', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1F1
                };
                Ext.apply(store.proxy.extraParams, np);
            }
            //,load: function (store, records, successful, operation, eOpts) {
            //    if (records.length > 0)
            //        T2Tool.down('#t2export').setDisabled(false);
            //    else
            //        T2Tool.down('#t2export').setDisabled(true);
            //}
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                create: 'POST',
                read: 'POST',
                update: 'PUT',
                delete: 'DELETE'
            },
            url: T11Get,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    function T2Load() {
        try {
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }

    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: '記錄編號',
            name: 'IDNO',
            fieldCls: 'readOnly',
            readOnly: true
        }, {
            fieldLabel: '參數名稱',
            name: 'PN',
            readOnly: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '參數值',
            name: 'PV',
            readOnly: true
        }]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        //autoScroll: true,
        cls: 'T2',
        //defaults: {
        //    layout: 'fit'
        //},
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],

        columns: [
            { xtype: 'rownumberer' },
            {
                text: "參數名稱",
                dataIndex: 'PN',
                width: 150
            }, {
                text: "參數值",
                dataIndex: 'PV',
                width: 600
            },
            { sortable: false, flex: 1 }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false); T2Form.setVisible(true);
                    }
                }
            },

            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
        }
        else {
            T2Form.getForm().reset();
        }
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
                        height: '50%',
                        items: [T1Grid]
                    }, {
                        region: 'south',
                        layout: 'fit',
                        title: T11Name,
                        collapsible: true,
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
