Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1GetExcel = '../../../api/AA0192/Excel';

    var st_storeloc = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_expdate = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_e_sourcecode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var st_orderkind = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0192/GetStoreLocCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var storelocs = data.etts;
                    if (storelocs.length > 0) {
                        for (var i = 0; i < storelocs.length; i++) {
                            st_storeloc.add({ VALUE: storelocs[i].VALUE, TEXT: storelocs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: '/api/AA0192/GetExpDateCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var expdates = data.etts;
                    if (expdates.length > 0) {
                        for (var i = 0; i < expdates.length; i++) {
                            st_expdate.add({ VALUE: expdates[i].VALUE, TEXT: expdates[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: '/api/AA0192/GetMatClassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasss = data.etts;
                    if (matclasss.length > 0) {
                        for (var i = 0; i < matclasss.length; i++) {
                            st_matclass.add({ VALUE: matclasss[i].VALUE, TEXT: matclasss[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: '/api/AA0192/GetMatClassSubCombo',
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
            url: '/api/AA0192/GetESourceCodeCombo',
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
            url: '/api/AA0192/GetMContidCombo',
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
            url: '/api/AA0192/GetOrderkindCombo',
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

    }
    setComboData();

    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P0',
        name: 'P0',
        fieldLabel: '藥材代碼',
        labelWidth: 100,
        width: 300,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0192/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    var T1QueryPhVenderCombo = Ext.create('WEBAPP.form.PhVenderCombo', {
        id: 'P3',
        name: 'P3',
        fieldLabel: '廠商代碼',
        labelWidth: 100,
        width: 300,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0192/GetPHVenderCombo',
        //查詢完會回傳的欄位
        extraFields: ['AGEN_NO', 'EASYNAME', 'AGEN_NAMEC'],
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
        autoScroll: true,
        layout: 'vbox',
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
                        T1QuerymmCodeCombo, //P0
                        {
                            xtype: 'combo',
                            fieldLabel: '倉儲儲位',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_storeloc,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '末效期',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_expdate,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        T1QueryPhVenderCombo, //P3
                        {
                            xtype: 'tbspacer',
                            width: 10
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'T1btn1',
                            handler: function () {
                                if (T1Query.getForm().isValid()) {
                                    T1Tool.down('#export').setDisabled(true);
                                    T1Load();
                                }
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
                            fieldLabel: '藥材分類',
                            name: 'P4',
                            id: 'P4',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_matclass,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '買斷寄庫',
                            name: 'P5',
                            id: 'P5',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_e_sourcecode,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '是否合約',
                            name: 'P6',
                            id: 'P6',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_m_contid,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '藥材類別',
                            name: 'P7',
                            id: 'P7',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_matclasssub,
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
                            fieldLabel: '採購類別',
                            name: 'P8',
                            id: 'P8',
                            labelWidth: 100,
                            width: 300,
                            emptyText: '全部',
                            store: st_orderkind,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE'
                        },
                        {
                            xtype: 'tbspacer',
                            width: 50
                        }, {
                            xtype: 'checkboxfield',
                            boxLabel: '刪除品項',
                            name: 'P9',
                            id: 'P9',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            inputValue: '1',
                            checked: false
                        },
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '特殊藥品',
                            name: 'P10',
                            id: 'P10',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            inputValue: '1',
                            checked: false
                        },
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '急救品項',
                            name: 'P11',
                            id: 'P11',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            inputValue: '1',
                            checked: false
                        },
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '只顯示戰備品項',
                            name: 'P12',
                            id: 'P12',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 100,
                            width: 110,
                            inputValue: '1',
                            checked: false
                        },
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '使用優惠價',
                            name: 'P13',
                            id: 'P13',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            inputValue: '1',
                            checked: false
                        },
                        {
                            xtype: 'checkboxfield',
                            boxLabel: '顯示負數量',
                            name: 'P14',
                            id: 'P14',
                            style: 'margin:0px 5px 0px 5px;',
                            labelWidth: 80,
                            width: 90,
                            inputValue: '1',
                            checked: false
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
            { name: 'F13', type: 'string' }
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
            url: '/api/AA0192/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#export').setDisabled(false);
                    }
                    else {
                        msglabel('查無資料!');
                        T1Tool.down('#export').setDisabled(true);
                    }
                }

            }
        }
    });

    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Store.getProxy().setExtraParam("p7", T1Query.getForm().findField('P7').getValue());
        T1Store.getProxy().setExtraParam("p8", T1Query.getForm().findField('P8').getValue());
        T1Store.getProxy().setExtraParam("p9", T1Query.getForm().findField('P9').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p10", T1Query.getForm().findField('P10').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p11", T1Query.getForm().findField('P11').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p12", T1Query.getForm().findField('P12').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p13", T1Query.getForm().findField('P13').checked ? 'Y' : 'N');
        T1Store.getProxy().setExtraParam("p14", T1Query.getForm().findField('P14').checked ? 'Y' : 'N');
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
                    p.push({ name: 'FN', value: '倉儲存量明細' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() });
                    p.push({ name: 'p8', value: T1Query.getForm().findField('P8').getValue() });
                    p.push({ name: 'p9', value: T1Query.getForm().findField('P9').checked ? 'Y' : 'N' });
                    p.push({ name: 'p10', value: T1Query.getForm().findField('P10').checked ? 'Y' : 'N' });
                    p.push({ name: 'p11', value: T1Query.getForm().findField('P11').checked ? 'Y' : 'N' });
                    p.push({ name: 'p12', value: T1Query.getForm().findField('P12').checked ? 'Y' : 'N' });
                    p.push({ name: 'p13', value: T1Query.getForm().findField('P13').checked ? 'Y' : 'N' });
                    p.push({ name: 'p14', value: T1Query.getForm().findField('P14').checked ? 'Y' : 'N' });
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
            { text: "藥材代碼", dataIndex: 'F1', width: 100 },
            { text: "藥材名稱", dataIndex: 'F2', width: 200 },
            { text: "倉儲儲位", dataIndex: 'F3', width: 100 },
            { text: "末效期", dataIndex: 'F4', width: 100 },
            { text: "現量", dataIndex: 'F5', width: 120, style: 'text-align:left', align: 'right' },
            { text: "總量", dataIndex: 'F6', width: 100, style: 'text-align:left', align: 'right' },
            { text: "戰備量", dataIndex: 'F7', width: 100, style: 'text-align:left', align: 'right' },
            { text: "單位", dataIndex: 'F8', width: 80 },
            { text: "單價", dataIndex: 'F9', width: 100, style: 'text-align:left', align: 'right' },
            { text: "戰備存量", dataIndex: 'F7', width: 100, style: 'text-align:left', align: 'right' },
            { text: "訂購數量", dataIndex: 'F9', width: 100, style: 'text-align:left', align: 'right' },
            { text: "小計", dataIndex: 'F10', width: 100, style: 'text-align:left', align: 'right' },
            { text: "買斷寄庫", dataIndex: 'F11', width: 100 },
            { text: "廠商", dataIndex: 'F12', width: 100 },
            { text: "批號", dataIndex: 'F13', width: 100 },
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
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});