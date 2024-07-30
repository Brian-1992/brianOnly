Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_docno = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0185/GetDocnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            },
        },
        autoLoad: true
    });

    var st_inid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0185/GetInidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });

    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P6',
        name: 'P6',
        fieldLabel: '院內碼',
        labelWidth: 120,
        width: 330,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0185/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    var T22QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'T22P6',
        name: 'T22P6',
        fieldLabel: '院內碼',
        labelWidth: 120,
        width: 330,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0185/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        autoScroll: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 120,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [{
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '補藥通知編號區間',
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 120,
                            width: 280,
                            store: st_docno,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '~',
                            name: 'P1',
                            id: 'P1',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 170,
                            store: st_docno,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '補藥日期區間',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 120,
                            width: 220
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '~',
                            name: 'P3',
                            id: 'P3',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 110
                        },
                        {
                            xtype: 'checkbox',
                            fieldLabel: '顯示取消',
                            name: 'P7'

                        },
                        {
                            xtype: 'tbspacer',
                            width: 10
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            handler: function () {
                                T1Load();
                            }
                        }, {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }, {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                        items: [
                            {
                                xtype: 'combo',
                                fieldLabel: '補藥單位區間',
                                name: 'P4',
                                id: 'P4',
                                labelWidth: 120,
                                width: 280,
                                store: st_inid,
                                queryMode: 'local',
                                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }, {
                                xtype: 'combo',
                                fieldLabel: '~',
                                name: 'P5',
                                id: 'P5',
                                labelSeparator: '',
                                labelWidth: 10,
                                width: 170,
                                store: st_inid,
                                queryMode: 'local',
                                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            T1QuerymmCodeCombo,
                            {
                            xtype: 'tbspacer',
                            width: 10
                            }
                        ]
                    }
                ]
            }
        ]
    });

    var T21Query = Ext.widget({
        xtype: 'form',
        frame: false,
        autoScroll: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 120,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [{
                    xtype: 'panel',
                    id: 'T21PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '補藥通知編號區間',
                            name: 'T21P0',
                            id: 'T21P0',
                            labelWidth: 120,
                            width: 280,
                            store: st_docno,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '~',
                            name: 'T21P1',
                            id: 'T21P1',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 170,
                            store: st_docno,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '補藥日期區間',
                            name: 'T21P2',
                            id: 'T21P2',
                            labelWidth: 120,
                            width: 220
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '~',
                            name: 'T21P3',
                            id: 'T21P3',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 110
                        }, {
                            xtype: 'tbspacer',
                            width: 10
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T21btn1',
                            handler: function () {
                                T21Load();
                            }
                        }, {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('T21P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }, {
                    xtype: 'panel',
                    id: 'T21PanelP2',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '補藥單位區間',
                            name: 'T21P4',
                            id: 'T21P4',
                            labelWidth: 120,
                            width: 280,
                            store: st_inid,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '~',
                            name: 'T21P5',
                            id: 'T21P5',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 170,
                            store: st_inid,
                            queryMode: 'local',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'tbspacer',
                            width: 10
                        }
                    ]
                }
                ]
            }
        ]
    });

    var T22Query = Ext.widget({
        xtype: 'form',
        frame: false,
        autoScroll: false,
        layout: 'vbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 120,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [{
                    xtype: 'panel',
                    id: 'T22PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        T22QuerymmCodeCombo,
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T22btn1',
                            handler: function () {
                                T22Load();
                            }
                        }, {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('T22P6').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }]
            }
        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F9', type: 'string' },
            { name: 'F10', type: 'string' },
            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' }
        ]
    });

    Ext.define('T21Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F3', type: 'string' },
            { name: 'F4', type: 'string' },
            { name: 'F5', type: 'string' },
        ]
    });

    Ext.define('T22Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
            { name: 'F2', type: 'string' },
            { name: 'F6', type: 'string' },
            { name: 'F7', type: 'string' },
            { name: 'F8', type: 'string' },
            { name: 'F9', type: 'string' },
            { name: 'F10', type: 'string' },
            { name: 'F11', type: 'string' },
            { name: 'F12', type: 'string' },
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }, { property: 'F2', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0185/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {   

            }
        }
    });

    var T21Store = Ext.create('Ext.data.Store', {
        model: 'T21Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0185/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {

            }
        }
    });

    var T22Store = Ext.create('Ext.data.Store', {
        model: 'T22Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F2', direction: 'ASC' }, { property: 'F7', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0185/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            beforeload: function (store, options) {
                if (T21LastRec) {
                    var np = {
                        p0: T21LastRec.data.F1,
                        p1: T22Query.getForm().findField('T22P6').getValue()
                    };
                    Ext.apply(store.proxy.extraParams, np);
                }
                else {
                    T22Store.removeAll();
                    return false;
                }
            }
        },
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').rawValue);
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').rawValue);
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').getValue());
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    function T21Load() {
        T21Store.getProxy().setExtraParam("p0", T21Query.getForm().findField('T21P0').getValue());
        T21Store.getProxy().setExtraParam("p1", T21Query.getForm().findField('T21P1').getValue());
        T21Store.getProxy().setExtraParam("p2", T21Query.getForm().findField('T21P2').rawValue);
        T21Store.getProxy().setExtraParam("p3", T21Query.getForm().findField('T21P3').rawValue);
        T21Store.getProxy().setExtraParam("p4", T21Query.getForm().findField('T21P4').getValue());
        T21Store.getProxy().setExtraParam("p5", T21Query.getForm().findField('T21P5').getValue());
        T21Tool.moveFirst();
        msglabel('訊息區:');
    }

    function T22Load() {
        T22Store.load({
            params: {
                start: 0
            }
        });
        T22Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        //buttons: []
    });

    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        //buttons: []
    });

    var T22Tool = Ext.create('Ext.PagingToolbar', {
        store: T22Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        //buttons: []
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
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
        columns: [
            { xtype: 'rownumberer' },
            { text: "補藥通知編號", dataIndex: 'F1', width: 120 },
            { text: "項次", dataIndex: 'F2', width: 60 },
            { text: "補藥日期", dataIndex: 'F3', width: 100 },
            { text: "補藥單位", dataIndex: 'F4', width: 150 },
            { text: "補藥通知人員", dataIndex: 'F5', width: 110 },
            { text: "核可人員", dataIndex: 'F6', width: 100 },
            { text: "藥材代碼", dataIndex: 'F7', width: 100 },
            { text: "藥材名稱", dataIndex: 'F8', width: 200 },
            { text: "藥材單位", dataIndex: 'F9', width: 100 },
            { text: "補藥量", dataIndex: 'F10', width: 100, style: 'text-align:left', align: 'right' },
            { text: "核可補藥量", dataIndex: 'F11', width: 100, style: 'text-align:left', align: 'right' },
            { text: "執行情況", dataIndex: 'F12', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
        }
    });

    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store, 
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T21Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T21Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "補藥通知編號", dataIndex: 'F1', width: 120 },
            { text: "補藥日期", dataIndex: 'F3', width: 100 },
            { text: "補藥單位", dataIndex: 'F4', width: 150 },
            { text: "補藥通知人員", dataIndex: 'F5', width: 110 },
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T21Rec = records.length;
                T21LastRec = records[0];
                T22Load();
            }
        }
    });

    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T22Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T22Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "項次", dataIndex: 'F2', width: 60 },
            { text: "藥材代碼", dataIndex: 'F7', width: 100 },
            { text: "藥材名稱", dataIndex: 'F8', width: 200 },
            { text: "藥材單位", dataIndex: 'F9', width: 100 },
            { text: "補藥量", dataIndex: 'F10', width: 100, style: 'text-align:left', align: 'right' },
            { text: "核可補藥量", dataIndex: 'F11', width: 100, style: 'text-align:left', align: 'right' },
            { text: "核可人員", dataIndex: 'F6', width: 100 },
            { text: "執行情況", dataIndex: 'F12', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
        }
    });
    
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "未展開":
                        break;
                    case "已展開":
                        break;
                }
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            title: '未展開',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid],
            }]
        },
        {
            title: '已展開',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 'T21Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [{
                    region: 'center',
                    layout: {
                        type: 'border',
                        padding: 0
                    },
                    collapsible: false,
                    title: '',
                    split: true,
                    flex: 1,
                    minWidth: 50,
                    minHeight: 140,
                    items: [
                        {
                            region: 'north',
                            layout: 'fit',
                            collapsible: false,
                            title: '',
                            split: true,
                            height: '45%',
                            items: [T21Grid]
                        }, {
                            region: 'north',
                            layout: 'fit',
                            collapsible: false,
                            //title: '明細',
                            split: true,
                            height: '55%',
                            items: [T22Grid]
                        }
                    ]
                }]
            }
            ]
        }
        ]
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: TATabs
    });

    T1Query.getForm().findField('P0').focus();
});