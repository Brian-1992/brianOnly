
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var T1Name = "存量查詢報表";
    var UrlReport = '/Report/F/FA0079.aspx';
    var T1GetExcel = '/api/FA0079/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAB = Ext.getUrlParam('isAB');

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
    var st_touchcase = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_e_restrictcode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        //P2
        Ext.Ajax.request({
            url: '/api/FA0079/GetMatClassSubCombo',
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
        //P4
        Ext.Ajax.request({
            url: '/api/FA0079/GetESourceCodeCombo',
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
        //P5
        Ext.Ajax.request({
            url: '/api/FA0079/GetMContidCombo',
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
        //P6
        Ext.Ajax.request({
            url: '/api/FA0079/GetOrderCodeCombo',
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
        //P7
        Ext.Ajax.request({
            url: '/api/FA0079/GetTouchCaseCombo',
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
        //P8
        Ext.Ajax.request({
            url: '/api/FA0079/GetERestrictcodeCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var e_restrictcode = data.etts;
                    if (e_restrictcode.length > 0) {
                        for (var i = 0; i < e_restrictcode.length; i++) {
                            st_e_restrictcode.add({ VALUE: e_restrictcode[i].VALUE, TEXT: e_restrictcode[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();
    var T1QueryWhNoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P1',
        id: 'P1',
        fieldLabel: '庫房代碼',
        emptyText: '全院',
        //fieldCls: 'required',
        //allowBlank: false,
        width: 290,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/FA0079/GetWhmastCombo',//指定查詢的Controller路徑
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                isab: isAB
            };
        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P3',
        id: 'P3',
        fieldLabel: '藥材代碼',
        labelAlign: 'right',
        emptyText: '全部',
        width: 290,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/FA0079/GetMMCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                //p1: T1Form.getForm().findField('DOCNO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        frame: false,
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80,
            labelAlign: 'right',
            style: 'text-align:center'
        },
        border: false,

        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        id: 'PanelP0',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'monthfield',
                                fieldLabel: '月結年月',
                                name: 'P0',
                                id: 'P0',
                                labelWidth: 80,
                                width: 290,
                                fieldCls: 'required',
                                value: new Date(),
                                allowBlank: false
                            },
                            T1QueryWhNoCombo, //P1
                            {
                                xtype: 'combo',
                                fieldLabel: '藥材類別',
                                name: 'P2',
                                id: 'P2',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_matclasssub,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP3',
                        border: false,
                        layout: 'hbox',
                        items: [
                            T1QueryMMCode, //P3
                            {
                                xtype: 'combo',
                                fieldLabel: '買斷或寄庫',
                                name: 'P4',
                                id: 'P4',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_e_sourcecode,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '是否合約',
                                name: 'P5',
                                id: 'P5',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_m_contid,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP6',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'combo',
                                fieldLabel: '採購類別',
                                name: 'P6',
                                id: 'P6',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_orderkind,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '合約類別',
                                name: 'P7',
                                id: 'P7',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_touchcase,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '管制品項',
                                name: 'P8',
                                id: 'P8',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_e_restrictcode,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'checkboxgroup',
                                name: 'P9',
                                id: 'P9',
                                width: 200,
                                items: [
                                    { boxLabel: '存量不為0', name: 'rb', inputValue: '1' },
                                    { boxLabel: '進出不為0', name: 'rb', inputValue: '2' }
                                ]
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP9',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    if (T1Query.getForm().isValid()) {
                                        var f = T1Query.getForm();
                                        objMask.show();
                                        T1Load();
                                        T2Load();
                                    }
                                    else {
                                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>請輸入必填欄位</span>');
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
            { name: 'F17', type: 'string' },
            { name: 'F18', type: 'string' },
            { name: 'F19', type: 'string' },
            { name: 'F20', type: 'string' },
            { name: 'F21', type: 'string' },
            { name: 'F22', type: 'string' },
            { name: 'F23', type: 'string' },
            { name: 'F24', type: 'string' },
            { name: 'F25', type: 'string' },
            { name: 'F26', type: 'string' },
            { name: 'F27', type: 'string' },
            { name: 'F28', type: 'string' },
            { name: 'F29', type: 'string' },
            { name: 'F30', type: 'string' },
            { name: 'F31', type: 'string' },
            { name: 'F32', type: 'string' },
            { name: 'F33', type: 'string' },
            { name: 'F34', type: 'string' },
            { name: 'F35', type: 'string' },
            { name: 'F36', type: 'string' },
            { name: 'F37', type: 'string' },
            { name: 'F38', type: 'string' },
            { name: 'F39', type: 'string' },
            { name: 'F40', type: 'string' }
        ]

    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0079/AllM',
            timeout: 9000000,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var tmp = '';
                var checkBox = T1Query.getForm().findField('P9').getChecked();
                if (checkBox != null) {
                    for (var i = 0; i < checkBox.length; i++) {
                        if (i > 0)
                            tmp += ',';
                        tmp += checkBox[i].inputValue;
                    }
                }
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: tmp,
                    isab: isAB
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T1Tool.down('#export').setDisabled(false);
                        T1Tool.down('#print').setDisabled(false);
                        T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        T1Tool.down('#print').setDisabled(true);
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                    }
                }

                Ext.Ajax.request({
                    url: '/api/FA0079/GetExtraDiscAmout',
                    params: {
                        p0: T1Query.getForm().findField('P0').rawValue
                    },
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            T2Form.getForm().findField('F20').setValue(data.msg);
                        }
                    },
                    failure: function (response, options) {

                    }
                });
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'print', text: '列印', disabled: true,
                handler: function () {
                    showReport();
                }
            },
            {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            var tmp = '';
                            var checkBox = T1Query.getForm().findField('P9').getChecked();
                            if (checkBox != null) {
                                for (var i = 0; i < checkBox.length; i++) {
                                    if (i > 0)
                                        tmp += ',';
                                    tmp += checkBox[i].inputValue;
                                }
                            }
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //SQL篩選條件
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                            p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件
                            p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() }); //SQL篩選條件
                            p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() }); //SQL篩選條件
                            p.push({ name: 'p8', value: T1Query.getForm().findField('P8').getValue() }); //SQL篩選條件
                            p.push({ name: 'p9', value: tmp }); //SQL篩選條件
                            p.push({ name: 'isab', value: isAB }); //SQL篩選條件                       
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }

        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        //loadingText: '處理中...',
        //loadMask: true,
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
            { text: "庫房代碼", dataIndex: 'F1', width: 200 },
            { text: "藥材代碼", dataIndex: 'F2', width: 100 },
            { text: "藥材名稱", dataIndex: 'F3', width: 200 },
            { text: "上月單價", dataIndex: 'F4', width: 110, style: 'text-align:left', align: 'right' },
            { text: "上月優惠單價", dataIndex: 'F5', width: 130, style: 'text-align:left', align: 'right' },
            { text: "目前單價", dataIndex: 'F6', width: 110, style: 'text-align:left', align: 'right' },
            { text: "目前優惠單價", dataIndex: 'F7', width: 130, style: 'text-align:left', align: 'right' },
            { text: "實存量", dataIndex: 'F8', width: 100, style: 'text-align:left', align: 'right' },
            { text: "應存量", dataIndex: 'F9', width: 100, style: 'text-align:left', align: 'right' },
            { text: "上月結存", dataIndex: 'F10', width: 110, style: 'text-align:left', align: 'right' },
            { text: "進貨量", dataIndex: 'F11', width: 100, style: 'text-align:left', align: 'right' },
            { text: "退貨量", dataIndex: 'F12', width: 100, style: 'text-align:left', align: 'right' },
            { text: "撥發量", dataIndex: 'F13', width: 100, style: 'text-align:left', align: 'right' },
            { text: "退料量", dataIndex: 'F14', width: 100, style: 'text-align:left', align: 'right' },
            { text: "消耗量", dataIndex: 'F15', width: 100, style: 'text-align:left', align: 'right' },
            { text: "出貨量", dataIndex: 'F16', width: 100, style: 'text-align:left', align: 'right' },
            { text: "轉換量比", dataIndex: 'F17', width: 100, style: 'text-align:left', align: 'right' },
            { text: "單位", dataIndex: 'F18', width: 100 },
            { text: "庫存成本", dataIndex: 'F19', width: 110, style: 'text-align:left', align: 'right' },
            { text: "應有庫存成本", dataIndex: 'F20', width: 130, style: 'text-align:left', align: 'right' },
            { text: "廠商代碼", dataIndex: 'F21', width: 100 },
            { text: "廠商簡稱", dataIndex: 'F22', width: 100 },
            { text: "廠商名稱", dataIndex: 'F23', width: 150 },
            { text: "管制品", dataIndex: 'F24', width: 100 },
            { text: "買斷寄庫", dataIndex: 'F25', width: 100 },
            { text: "藥材類別", dataIndex: 'F26', width: 100 },
            { text: "合約方式", dataIndex: 'F27', width: 100 },
            { text: "案號", dataIndex: 'F28', width: 100 },
            { text: "合約到期日", dataIndex: 'F29', width: 100 },
            { text: "合約類別", dataIndex: 'F30', width: 100 },
            { text: "優惠比", dataIndex: 'F31', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "調撥入庫", dataIndex: 'F32', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "調撥出庫", dataIndex: 'F33', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "繳回出庫總量", dataIndex: 'F34', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "報廢總量", dataIndex: 'F35', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "換貨入庫總量", dataIndex: 'F36', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "換貨出庫總量", dataIndex: 'F37', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "盤點差異量", dataIndex: 'F38', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "儲位", dataIndex: 'F39', width: 130, style: 'text-align:left', align: 'righht' },
            { text: "上級庫存量", dataIndex: 'F40', width: 130, style: 'text-align:left', align: 'righht' },
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

    Ext.define('T2Model', {
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
            { name: 'F17', type: 'string' },
            { name: 'F18', type: 'string' },
            { name: 'F19', type: 'string' }
        ]

    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'F1', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0079/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                var tmp = '';
                var checkBox = T1Query.getForm().findField('P9').getChecked();
                if (checkBox != null) {
                    for (var i = 0; i < checkBox.length; i++) {
                        if (i > 0)
                            tmp += ',';
                        tmp += checkBox[i].inputValue;
                    }
                }
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: tmp,
                    isab: isAB
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T2Store.removeAll();
                }
                else {
                    if (records.length > 0) {
                        T2Form.loadRecord(records[0]);
                        objMask.hide();
                    }
                    else {
                        msglabel('查無資料!');
                        Ext.Msg.alert('提醒', '查無資料!');
                        T2Form.getForm().reset();
                    }
                }
            }
        }
    });
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }
    var mLabelWidth = 160;
    var mWidth = 360;
    var T2Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        layout: {
            type: 'table',
            columns: 4,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [

            { fieldLabel: '上月結存', name: 'F1', readOnly: true, width: mWidth },
            { fieldLabel: '本月進貨', name: 'F6', readOnly: true, width: mWidth },
            { fieldLabel: '本月退貨', name: 'F11', readOnly: true, width: mWidth },
            { fieldLabel: '本月贈品', name: 'F16', readOnly: true, width: mWidth },

            { fieldLabel: '消耗金額', name: 'F2', readOnly: true, width: mWidth },
            { fieldLabel: '軍方消耗', name: 'F7', readOnly: true, width: mWidth },
            { fieldLabel: '民眾消耗', name: 'F12', readOnly: true, width: mWidth },
            { fieldLabel: '退料金額', name: 'F17', readOnly: true, width: mWidth },

            { fieldLabel: '應有結存', name: 'F3', readOnly: true, width: mWidth },
            { fieldLabel: '盤點結存', name: 'F8', readOnly: true, width: mWidth },
            { fieldLabel: '本月結存', name: 'F13', readOnly: true, width: mWidth },
            { fieldLabel: '差異金額', name: 'F18', readOnly: true, width: mWidth },

            { fieldLabel: '買斷結存', name: 'F4', readOnly: true, width: mWidth },
            { fieldLabel: '上月寄庫藥品買斷結存', name: 'F9', readOnly: true, width: mWidth },
            { fieldLabel: '本月寄庫藥品買斷結存', name: 'F14', readOnly: true, width: mWidth },
            { fieldLabel: '戰備金額', name: 'F19', readOnly: true, width: mWidth },

            { fieldLabel: '寄庫結存', name: 'F5', readOnly: true, width: mWidth },
            { fieldLabel: '不含戰備上月結存價差', name: 'F10', readOnly: true, width: mWidth },
            { fieldLabel: '戰備本月價差', name: 'F15', readOnly: true, width: mWidth },
            { fieldLabel: '折讓金額', name: 'F20', readOnly: true, width: mWidth }

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
        items: [
            {
                region: 'north',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                height: '70%',
                items: [T1Grid]
            },
            {
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                height: '30%',
                split: true,
                items: [T2Form]
            }
        ]
    });

    function showReport() {
        if (!win) {
            var tmp = '';
            var checkBox = T1Query.getForm().findField('P9').getChecked();
            if (checkBox != null) {
                for (var i = 0; i < checkBox.length; i++) {
                    if (i > 0)
                        tmp += ',';
                    tmp += checkBox[i].inputValue;
                }
            }
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P1=' + T1Query.getForm().findField('P1').getValue()
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P3=' + T1Query.getForm().findField('P3').getValue()
                + '&P4=' + T1Query.getForm().findField('P4').getValue()
                + '&P5=' + T1Query.getForm().findField('P5').getValue()
                + '&P6=' + T1Query.getForm().findField('P6').getValue()
                + '&P7=' + T1Query.getForm().findField('P7').getValue()
                + '&P8=' + T1Query.getForm().findField('P8').getValue()
                + '&P9=' + tmp
                + '&isab=' + isAB
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
    }
    var objMask = new Ext.LoadMask({
        target: T2Form,
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