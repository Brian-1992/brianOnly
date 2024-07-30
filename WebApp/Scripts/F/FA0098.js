
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Name = "民眾衛材科成本分攤表";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        border: false,
        items: [{
            xtype: 'monthfield',
            fieldLabel: '月結年月',
            name: 'P0',
            id: 'P0',
            labelAlign: 'right',
            labelWidth: 80,
            width: 180,
            padding: 10,
            fieldCls: 'required',
            value: new Date(),
            allowBlank: false
        }, {
            xtype: 'button',
            text: '查詢',
            margin: '10 2 10',
            handler: function () {
                if (T1Query.getForm().isValid()) {
                    var f = T1Query.getForm();
                    objMask.show();
                    T1Load();
                    T4Load();
                    T5Load();
                }
                else {
                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
                }
            }
        }, {
            xtype: 'button',
            text: '清除',
            margin: '10 2 10',
            handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    Ext.define('T1FormModel', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F_A', type: 'string' },
            { name: 'F_B', type: 'string' },
            { name: 'F_C', type: 'string' },
            { name: 'F_D', type: 'string' },
            { name: 'F_E', type: 'string' },
            { name: 'F_F', type: 'string' },
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F33', type: 'string' },
            { name: 'F34', type: 'string' },
            { name: 'F9', type: 'string' },
            { name: 'F10', type: 'string' },
            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' },
            { name: 'F13', type: 'string' },
            { name: 'F14', type: 'string' },
            { name: 'F15', type: 'string' },
            { name: 'F16', type: 'string' },
            { name: 'F37', type: 'string' },
            { name: 'F17', type: 'string' },
            { name: 'F18', type: 'string' },
            { name: 'F19', type: 'string' },
            { name: 'F20', type: 'string' },
            { name: 'F21', type: 'string' },
            { name: 'F22', type: 'string' },
            { name: 'F23', type: 'string' },
            { name: 'F24', type: 'string' },
            { name: 'F35', type: 'string' },
            { name: 'F36', type: 'string' },
            { name: 'F25', type: 'string' },
            { name: 'F26', type: 'string' },
            { name: 'F27', type: 'string' },
            { name: 'F28', type: 'string' },
            { name: 'F29', type: 'string' },
            { name: 'F30', type: 'string' },
            { name: 'F31', type: 'string' },
            { name: 'F32', type: 'string' },
            { name: 'F38', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1FormModel',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F_A', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0098/AllForm',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Form.loadRecord(records[0]);
                        T2Form.loadRecord(records[0]);
                        T3Form.loadRecord(records[0]);
                        objMask.hide();
                    }
                    else {
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T1Form.getForm().reset();
                        T2Form.getForm().reset();
                        T3Form.getForm().reset();
                    }
                }
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
    }
    var T1Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        layout: {
            type: 'table',
            columns: 5
        },
        padding: '5 0 0',
        border: 0,
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: 100,
            width: 250,
            padding: '0',
            xtype: 'textfield'
        },
        items: [
            { fieldLabel: '全院購入總值', name: 'F_A' },
            { fieldLabel: '藥品購入總值', name: 'F_B' },
            { fieldLabel: '衛材購入總額', name: 'F_C' },
            { fieldLabel: '全院消耗成本', name: 'F_D' },
            { fieldLabel: '藥品消耗成本', name: 'F_E' },
            { fieldLabel: '衛材消耗成本', name: 'F_F' },
            { fieldLabel: '藥品及衛材消耗成本計算方式=購入總值+上月實際盤存-本月實際盤存(如下表)', xtype: 'displayfield', labelSeparator: '', labelWidth: 500, colspan: 2 },
            { fieldLabel: '備註', width: 500, colspan: 2 }
        ]
    });

    var T2Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        layout: 'vbox',
        padding: '1 5 1',
        border: 0,
        autoScroll: true,
        frame: false,
        items: [{
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '【藥品支付明細】', xtype: 'displayfield', labelWidth: 120 }
            ]
        },
        {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '上月全院藥品實際盤存', name: 'F1', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F2', labelWidth: 75, width: 205 },
                { fieldLabel: '藥劑科等', name: 'F3', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F4', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院藥品實際盤存', name: 'F5', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F6', labelWidth: 75, width: 205 },
                { fieldLabel: '藥劑科等', name: 'F7', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F8', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院藥品購入', name: 'F33', labelWidth: 150, width: 280 },
                { fieldLabel: '藥庫買斷購入-買斷折讓+寄庫購入', xtype: 'displayfield', labelWidth: 215 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院買斷藥品消耗', name: 'F34', labelWidth: 150, width: 280 },
                { fieldLabel: '消耗只列藥劑科之消耗(買斷消耗+寄庫消耗)', xtype: 'displayfield', labelWidth: 275 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院藥品帳面盤存', name: 'F9', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F10', labelWidth: 75, width: 205 },
                { fieldLabel: '藥劑科等', name: 'F11', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F12', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月藥品盤(盈)虧', name: 'F13', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F14', labelWidth: 75, width: 205 },
                { fieldLabel: '藥劑科等', name: 'F15', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F16', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院戰備藥品結存', name: 'F37', labelWidth: 150, width: 280 },
                { fieldLabel: '備註', labelWidth: 75, width: 500 }
            ]
        }
        ]
    });

    var T3Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        layout: 'vbox',
        padding: '1 5 1',
        border: 0,
        autoScroll: true,
        frame: false,
        items: [{
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '【衛材支付明細】', xtype: 'displayfield', labelWidth: 120 }
            ]
        },
        {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '上月全院衛材實際盤存', name: 'F17', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F18', labelWidth: 75, width: 205 },
                { fieldLabel: '使用單位', name: 'F19', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F20', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院衛材實際盤存', name: 'F21', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F22', labelWidth: 75, width: 205 },
                { fieldLabel: '使用單位', name: 'F23', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F24', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院衛材購入', name: 'F35', labelWidth: 150, width: 280 },
                { fieldLabel: '衛材庫買斷購入+寄庫購入-退貨-買斷折讓', xtype: 'displayfield', labelWidth: 260 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院衛材消耗', name: 'F36', labelWidth: 150, width: 280 },
                { fieldLabel: '全院買斷消耗+寄庫消耗(不含供應中心及衛材庫)', xtype: 'displayfield', labelWidth: 300 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院衛材帳面盤存', name: 'F25', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F26', labelWidth: 75, width: 205 },
                { fieldLabel: '使用單位', name: 'F27', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F28', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月衛材盤(盈)虧', name: 'F29', labelWidth: 150, width: 280 },
                { fieldLabel: '(各科護理等', name: 'F30', labelWidth: 75, width: 205 },
                { fieldLabel: '使用單位', name: 'F31', labelWidth: 60, width: 190 },
                { fieldLabel: '藥庫', name: 'F32', labelWidth: 35, width: 165 },
                { fieldLabel: ')', xtype: 'displayfield', labelWidth: 5 }
            ]
        }, {
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true,
                xtype: 'textfield',
                labelSeparator: ''
            },
            items: [
                { fieldLabel: '本月全院戰備衛材結存', name: 'F38', labelWidth: 150, width: 280 },
                { fieldLabel: '備註', labelWidth: 75, width: 500 }
            ]
        }
        ]
    });

    Ext.define('T4GridModel', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'SECTIONNO', type: 'string' },
            { name: 'SECTIONNAME', type: 'string' },
            { name: 'S8', type: 'string' }
        ]

    });
    var T4Store = Ext.create('Ext.data.Store', {
        model: 'T4GridModel',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'S8', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0098/AllGrid',
            timeout: 9000000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T4Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T4LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        //msglabel('查無資料!');
                        //Ext.Msg.alert('提醒', '查無資料!');
                    }
                }
                //Ext.Ajax.request({
                //    url: '/api/FA0098/GetExtraDiscAmout',
                //    params: {
                //        p0: T1Query.getForm().findField('P0').rawValue
                //    },
                //    method: reqVal_p,
                //    success: function (response) {
                //        var data = Ext.decode(response.responseText);
                //        if (data.success) {
                //            T2Form.getForm().findField('F20').setValue(data.msg);
                //        }
                //    },
                //    failure: function (response, options) {

                //    }
                //});
            }
        }
    });
    function T4Load() {
        T4Tool.moveFirst();
    }
    var T4Tool = Ext.create('Ext.PagingToolbar', {
        store: T4Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: []
    });
    var T4Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T4Store,
        plain: true,
        //loadingText: '處理中...',
        //loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T4Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "科別", dataIndex: 'SECTIONNO', width: 80 },
            { text: "科名", dataIndex: 'SECTIONNAME', width: 120 },
            { text: "金額", dataIndex: 'S8', width: 80 },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                //T1Rec = records.length;
                //T1LastRec = records[0];
            }
        }
    });

    Ext.define('T5FormModel', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'GridAmount', type: 'string' },
            { name: 'GridCount', type: 'string' }
        ]
    });
    var T5Store = Ext.create('Ext.data.Store', {
        model: 'T5FormModel',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'GridAmount', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0098/AmountForm',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T5Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T5Form.loadRecord(records[0]);
                    }
                    else {
                        T5Form.getForm().reset();
                    }
                }
            }
        }
    });
    function T5Load() {
        T5Store.load({
            params: {
                start: 0
            }
        });
    }
    var T5Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        layout: 'vbox',
        padding: '5 5 1',
        border: 0,
        autoScroll: true,
        frame: false,
        items: [{
            xtype: 'panel',
            layout: 'hbox',
            border: 0,
            defaults: {
                labelAlign: 'right',
                readOnly: true
            },
            items: [
                { fieldLabel: '合計', name: 'GridAmount', xtype: 'textfield', labelWidth: 40 },
                { fieldLabel: '單位數', name: 'GridCount', xtype: 'textfield', labelWidth: 60 }
            ]
        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true,
            collapsible: false,
            title: ''
        },
        items: [{
            region: 'north',
            layout: 'fit',
            border: 0,
            height: '6%',
            items: [T1Query]
        }, {
            region: 'north',
            layout: 'fit',
            height: '12%',
            items: [T1Form]
        }, {
            region: 'center',
            border: 0,
            width: '65%', height: '100%',
            layout: {
                type: 'border',
                padding: 0
            },
            defaults: {
                split: true,
                collapsible: false,
                border: 0,
                title: ''
            },
            items: [{
                region: 'north',
                layout: 'fit',
                height: '50%',
                items: [T2Form]
            },
            {
                region: 'center',
                layout: 'fit',
                height: '50%',
                items: [T3Form]
            }]
        }, {
            region: 'east',
            border: 0,
            width: '35%',
            layout: {
                type: 'border',
                padding: 0
            },
            defaults: {
                split: true,
                collapsible: false,
                title: ''
            },
            items: [{
                region: 'north',
                layout: 'fit',
                height: '92%',
                title: '民眾消耗衛材科成本分攤表',
                items: [T4Grid]
            },
            {
                region: 'center',
                layout: 'fit',
                height: '8%',
                items: [T5Form]
            }]
        }
        ]
    });

    var objMask = new Ext.LoadMask({
        target: viewport,
        msg: '讀取中...',
        hideMode: 'display',
        listeners: {
            beforedestroy: function (lmask) {
                //this.hide();
            },
            hide: function (lmask) {
                //this.hide();
            }
        }
    });
});