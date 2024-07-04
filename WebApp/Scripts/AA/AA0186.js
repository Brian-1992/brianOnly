Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1GetExcel = '../../../api/AA0186/Excel';

    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_mrstatus = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "未核可", "TEXT": "未核可" },
            { "VALUE": "已撥", "TEXT": "已撥" },
            { "VALUE": "待撥", "TEXT": "待撥" },
            { "VALUE": "不核撥", "TEXT": "不核撥" }
        ]
    });

    var st_e_sourcecode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_touchcase = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_orderkind = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_agen_no = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0186/GetMatClassSubCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            st_matclasssub.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0186/GetESourceCodeCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var e_sourcecode = data.etts;
                    if (e_sourcecode.length > 0) {
                        for (var i = 0; i < e_sourcecode.length; i++) {
                            st_e_sourcecode.add({ VALUE: e_sourcecode[i].VALUE, TEXT: e_sourcecode[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0186/GetMContidCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var m_contid = data.etts;
                    if (m_contid.length > 0) {
                        for (var i = 0; i < m_contid.length; i++) {
                            st_m_contid.add({ VALUE: m_contid[i].VALUE, TEXT: m_contid[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0186/GetTouchcaseCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var touchcase = data.etts;
                    if (touchcase.length > 0) {
                        for (var i = 0; i < touchcase.length; i++) {
                            st_touchcase.add({ VALUE: touchcase[i].VALUE, TEXT: touchcase[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0186/GetOrderkindCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var orderkind = data.etts;
                    if (orderkind.length > 0) {
                        for (var i = 0; i < orderkind.length; i++) {
                            st_orderkind.add({ VALUE: orderkind[i].VALUE, TEXT: orderkind[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });


        Ext.Ajax.request({
            url: '/api/AA0186/GetAgen_noCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var agen = data.etts;                    
                    if (agen.length > 0) {
                        for (var i = 0; i < agen.length; i++) {
                            st_agen_no.add({ VALUE: agen[i].VALUE, TEXT: agen[i].TEXT, COMBITEM: agen[i].VALUE + ' ' + agen[i].TEXT});
                        }                        
                    }
                }
            },
            failure: function (response, options) {

            }
        });

    }
    setComboData();
    /*
    var T1QueryUrInidCombo = Ext.create('WEBAPP.form.UrInidCombo', {
        id: 'P3',
        name: 'P3',
        fieldLabel: '請領單位',
        labelWidth: 100,
        width: 300,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0186/GetUrInidCombo',
        //查詢完會回傳的欄位
        extraFields: ['INID', 'INID_NAME'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });*/
    //庫房代碼
    var T1QueryWhnoCombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0186/GetWhnoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P5',
        name: 'P5',
        fieldLabel: '藥材代碼',
        labelWidth: 100,
        width: 300,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0186/GetMMCodeCombo',
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
        layout: 'form',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 100,
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
                            xtype: 'datefield',
                            fieldLabel: '查詢日期範圍',
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 100,
                            width: 200
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '~',
                            name: 'P1',
                            id: 'P1',
                            labelSeparator: '',
                            labelWidth: 10,
                            width: 100
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '請領單編號',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 100,
                            width: 300
                        }, {
                            xtype: 'combo',
                            store: T1QueryWhnoCombo,
                            fieldLabel: '請領單位',
                            name: 'P3',
                            id: 'P3',
                            labelWidth: 100,
                            width: 300,
                            queryMode: 'local',
                            labelAlign: 'right',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;{TEXT}</div></tpl>',
                            // T1QueryUrInidCombo, //P3
                        }, {
                            xtype: 'combo',
                            fieldLabel: '藥材類別',
                            name: 'P4',
                            id: 'P4',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_matclasssub,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'tbspacer',
                            width: 10
                        },
                    ]
                }, {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    items: [
                        T1QuerymmCodeCombo, //P5
                        {
                            xtype: 'combo',
                            fieldLabel: '請領單狀態',
                            name: 'P6',
                            id: 'P6',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_mrstatus,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '買斷寄庫',
                            name: 'P7',
                            id: 'P7',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_e_sourcecode,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '是否合約',
                            name: 'P8',
                            id: 'P8',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_m_contid,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'tbspacer',
                            width: 10
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP3',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '合約類別',
                            name: 'P9',
                            id: 'P9',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_touchcase,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'combo',
                            fieldLabel: '採購類別',
                            name: 'P10',
                            id: 'P10',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_orderkind,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '備註',
                            name: 'P11',
                            id: 'P11',
                            labelWidth: 100,
                            width: 300
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '廠商代碼',
                            name: 'P12',
                            id: 'P12',
                            labelWidth: 100,
                            width: 400,
                            emptyText: '全部',
                            store: st_agen_no,
                            queryMode: 'local',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                        },
                        {
                            xtype: 'tbspacer',
                            width: 40
                        }
                        , {
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
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP4',
                    border: false,
                    layout: 'hbox',
                    items: [
                    ]
                }
                ]
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
            { name: 'F12', type: 'string' },
            { name: 'F13', type: 'string' },
            { name: 'F14', type: 'string' },
            { name: 'F15', type: 'string' },
            { name: 'F16', type: 'string' },
            { name: 'F19', type: 'string' },
            { name: 'F20', type: 'string' },
            { name: 'F21', type: 'string' },
            { name: 'F22', type: 'string' },
            { name: 'F23', type: 'string' },
            { name: 'F24', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 30, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0186/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    T1Tool.down('#export').setDisabled(false);
                } else {
                    T1Tool.down('#export').setDisabled(true);
                }

            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').getValue());
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').getValue());
        T1Store.getProxy().setExtraParam("p9", T1Query.getForm().findField('P9').getValue());
        T1Store.getProxy().setExtraParam("p10", T1Query.getForm().findField('P10').getValue());
        T1Store.getProxy().setExtraParam("p11", T1Query.getForm().findField('P11').getValue());
        T1Store.getProxy().setExtraParam("p12", T1Query.getForm().findField('P12').getValue());
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出Excel', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '請領單明細' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() });
                    p.push({ name: 'p8', value: T1Query.getForm().findField('P8').getValue() });
                    p.push({ name: 'p10', value: T1Query.getForm().findField('P10').getValue() });
                    p.push({ name: 'p11', value: T1Query.getForm().findField('P11').getValue() });
                    p.push({ name: 'p12', value: T1Query.getForm().findField('P12').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        ]
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
            { text: "單位代碼", dataIndex: 'F1', width: 100 },
            { text: "請領單位", dataIndex: 'F2', width: 100 },
            { text: "請領單", dataIndex: 'F3', width: 200 },
            { text: "請領單狀態", dataIndex: 'F19', width: 100 },
            { text: "請領日期", dataIndex: 'F4', width: 100 },
            { text: "藥材代碼", dataIndex: 'F5', width: 100 },
            { text: "請領數量", dataIndex: 'F6', width: 100, style: 'text-align:left', align: 'right' },
            { text: "核可量", dataIndex: 'F20', width: 100, style: 'text-align:left', align: 'right' },
            { text: "已撥發量", dataIndex: 'F21', width: 100, style: 'text-align:left', align: 'right' },
            { text: "單位", dataIndex: 'F7', width: 70 },
            { text: "單價", dataIndex: 'F8', width: 90, style: 'text-align:left', align: 'right' },
            { text: "小計", dataIndex: 'F9', width: 90, style: 'text-align:left', align: 'right' },
            { text: "軍民別", dataIndex: 'F10', width: 80 },
            { text: "買斷寄庫", dataIndex: 'F11', width: 100 },
            { text: "是否合約", dataIndex: 'F12', width: 100 },
            { text: "合約案號", dataIndex: 'F13', width: 100 },
            { text: "合約到期日", dataIndex: 'F14', width: 100 },
            { text: "合約類別", dataIndex: 'F15', width: 100 },
            { text: "備註", dataIndex: 'F16', width: 100 },
            { text: "採購類別", dataIndex: 'F17', width: 100 },
            { text: "藥材名稱", dataIndex: 'F18', width: 200 },
            { text: "病患身份證", dataIndex: 'F22', width: 100 },
            { text: "病患姓名", dataIndex: 'F23', width: 100 },
            { text: "明細備註", dataIndex: 'F24', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
        }
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

    T1Query.getForm().findField('P0').focus();
});