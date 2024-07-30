
Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {

    var T1Name = "低周轉率報表";
    var UrlReport = '/Report/F/FA0081.aspx';
    var T1GetExcel = '/api/FA0081/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAB = Ext.getUrlParam('isAB');

    //P3 藥材類別
    var st_matclasssub = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P5 類別
    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P6 買斷寄庫
    var st_e_sourcecode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P7 是否合約
    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P8 是否戰備
    var st_warbak = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P9 常備品
    var st_orderkind = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P10 刪除品項
    var st_cancel_id = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P11 特殊品項
    var st_spdrug = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    //P12 急救品項
    var st_fastdrug = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_e_restrictcode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        //P3 藥材類別
        Ext.Ajax.request({
            url: '/api/FA0081/GetMatClassSubCombo',
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
        //P5 類別
        Ext.Ajax.request({
            url: '/api/FA0081/GetMatClassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            st_matclass.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //P6 買斷寄庫
        Ext.Ajax.request({
            url: '/api/FA0081/GetESourceCodeCombo',
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
        //P7 是否合約
        Ext.Ajax.request({
            url: '/api/FA0081/GetMContidCombo',
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
        //P8 是否戰備
        Ext.Ajax.request({
            url: '/api/FA0081/GetWarbakCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var touchcase = data.etts;
                    if (touchcase.length > 0) {
                        for (var i = 0; i < touchcase.length; i++) {
                            st_warbak.add({ VALUE: touchcase[i].VALUE, TEXT: touchcase[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //P9 常備品
        Ext.Ajax.request({
            url: '/api/FA0081/GetOrderCodeCombo',
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
        //P10 刪除品項
        Ext.Ajax.request({
            url: '/api/FA0081/GetCancelIdCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var orderkind = data.etts;
                    if (orderkind.length > 0) {
                        for (var i = 0; i < orderkind.length; i++) {
                            st_cancel_id.add({ VALUE: orderkind[i].VALUE, TEXT: orderkind[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //P11 特殊品項
        Ext.Ajax.request({
            url: '/api/FA0081/GetSpdrugCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var orderkind = data.etts;
                    if (orderkind.length > 0) {
                        for (var i = 0; i < orderkind.length; i++) {
                            st_spdrug.add({ VALUE: orderkind[i].VALUE, TEXT: orderkind[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //P12 急救品項
        Ext.Ajax.request({
            url: '/api/FA0081/GetFastdrugCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var touchcase = data.etts;
                    if (touchcase.length > 0) {
                        for (var i = 0; i < touchcase.length; i++) {
                            st_fastdrug.add({ VALUE: touchcase[i].VALUE, TEXT: touchcase[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        //P8
        Ext.Ajax.request({
            url: '/api/FA0081/GetERestrictcodeCombo',
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
        name: 'P4',
        id: 'P4',
        fieldLabel: '單位',
        emptyText: '全院',
        //fieldCls: 'required',
        //allowBlank: false,
        width: 290,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/FA0081/GetWhmastCombo',//指定查詢的Controller路徑
        extraFields: ['WH_NO', 'WH_NAME'],
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            //return {
            //    isab: isAB
            //};
        },
        listeners: {
            select: function (c, r, i, e) {
            }
        }
    });
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P2',
        id: 'P2',
        fieldLabel: '藥材代碼',
        labelAlign: 'right',
        emptyText: '全部',
        width: 290,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0174/GetMMCodeCombo',
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
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
                                fieldLabel: '查詢月份',
                                name: 'P0',
                                id: 'P0',
                                labelWidth: 80,
                                width: 180,
                                fieldCls: 'required',
                                value: new Date(new Date().setMonth(new Date().getMonth() - 1)),
                                allowBlank: false
                            }, {
                                xtype: 'monthfield',
                                fieldLabel: '~',
                                name: 'P1',
                                id: 'P1',
                                labelSeparator: '',
                                enforceMaxLength: true,
                                labelWidth: 10,
                                width: 110,
                                fieldCls: 'required',
                                value: new Date(),
                                allowBlank: false
                            },
                            T1QueryMMCode, // 藥材代碼 P2
                            {
                                xtype: 'combo',
                                fieldLabel: '藥材類別',
                                name: 'P3',
                                id: 'P3',
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
                        id: 'PanelP4',
                        border: false,
                        layout: 'hbox',
                        items: [
                            T1QueryWhNoCombo, // 單位 P4
                            {
                                xtype: 'combo',
                                fieldLabel: '類別',
                                name: 'P5',
                                id: 'P5',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_matclass,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }, {
                                xtype: 'combo',
                                fieldLabel: '買斷寄庫',
                                name: 'P6',
                                id: 'P6',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_e_sourcecode,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP7',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'combo',
                                fieldLabel: '是否合約',
                                name: 'P7',
                                id: 'P7',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_m_contid,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '是否備戰',
                                name: 'P8',
                                id: 'P8',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_warbak,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '常備品',
                                name: 'P9',
                                id: 'P9',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_orderkind,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP10',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'combo',
                                fieldLabel: '刪除品項',
                                name: 'P10',
                                id: 'P10',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_cancel_id,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '特殊品項',
                                name: 'P11',
                                id: 'P11',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_spdrug,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            },
                            {
                                xtype: 'combo',
                                fieldLabel: '急救品項',
                                name: 'P12',
                                id: 'P12',
                                labelWidth: 80,
                                width: 290,
                                emptyText: '全部',
                                store: st_fastdrug,
                                queryMode: 'local',
                                displayField: 'TEXT',
                                valueField: 'VALUE'
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        id: 'PanelP9',
                        border: false,
                        layout: 'hbox',
                        items: [
                            {
                                xtype: 'container',
                                defaultType: 'checkboxfield',
                                layout: 'hbox',
                                items: [
                                    {
                                        boxLabel: '包含買斷與戰備',
                                        name: 'P13',
                                        inputValue: 'Y',
                                        labelAlign: 'right',
                                        id: 'P13',
                                        width: 110,
                                        padding: '0 15 0 85',
                                    }
                                ]
                            }, {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    if (T1Query.getForm().isValid()) {
                                        var f = T1Query.getForm();
                                        //objMask.show();
                                        T1Load();
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
                            }, {
                                xtype: 'tbspacer',
                                width: 50,
                            }, {
                                xtype: 'label',
                                text: '低周轉率定義：庫存量大於消耗量',
                                style: {
                                    color: 'red'
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
            { name: 'F22', type: 'string' }
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
            url: '/api/FA0081/AllM',
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
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: T1Query.getForm().findField('P9').getValue(),
                    p10: T1Query.getForm().findField('P10').getValue(),
                    p11: T1Query.getForm().findField('P11').getValue(),
                    p12: T1Query.getForm().findField('P12').getValue(),
                    p13: T1Query.getForm().findField('P13').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    var dataCount = store.getCount();
                    if (dataCount > 0) {
                        T1Tool.down('#export').setDisabled(false);
                        T1Tool.down('#print').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#export').setDisabled(true);
                        T1Tool.down('#print').setDisabled(true);
                    }
                    Ext.Ajax.request({
                        url: '/api/FA0081/GetExtraDiscAmout',
                        params: {
                            p0: T1Query.getForm().findField('P0').rawValue
                        },
                        method: reqVal_p,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                //T2Form.getForm().findField('F20').setValue(data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
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
                            p.push({ name: 'p0', value: T1Query.getForm().findField('P0').rawValue }); //SQL篩選條件
                            p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue }); //SQL篩選條件
                            p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                            p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                            p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件
                            p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() }); //SQL篩選條件
                            p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() }); //SQL篩選條件
                            p.push({ name: 'p8', value: T1Query.getForm().findField('P8').getValue() }); //SQL篩選條件    
                            p.push({ name: 'p9', value: T1Query.getForm().findField('P9').getValue() }); //SQL篩選條件    
                            p.push({ name: 'p10', value: T1Query.getForm().findField('P10').getValue() }); //SQL篩選條件    
                            p.push({ name: 'p11', value: T1Query.getForm().findField('P11').getValue() }); //SQL篩選條件    
                            p.push({ name: 'p12', value: T1Query.getForm().findField('P12').getValue() }); //SQL篩選條件    
                            p.push({ name: 'p13', value: T1Query.getForm().findField('P13').getValue() }); //SQL篩選條件    
                            PostForm(T1GetExcel, p);
                        }
                    });
                }
            }

        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
            { text: "月結月份", dataIndex: 'F1', width: 85, style: 'text-align:left', align: 'center'  },
            { text: "藥材代碼", dataIndex: 'F2', width: 100 },
            { text: "藥材名稱", dataIndex: 'F3', width: 300 },
            { text: "單位", dataIndex: 'F4', width: 80, style: 'text-align:left', align: 'center' },
            { text: "單價", dataIndex: 'F5', width: 90, style: 'text-align:left', align: 'right' },
            { text: "上月結存", dataIndex: 'F6', width: 90, style: 'text-align:left', align: 'right' },
            { text: "本月進貨", dataIndex: 'F7', width: 90, style: 'text-align:left', align: 'right' },
            { text: "應有結存量", dataIndex: 'F8', width: 100, style: 'text-align:left', align: 'right' },
            { text: "退貨量", dataIndex: 'F9', width: 90, style: 'text-align:left', align: 'right' },
            { text: "盤存量", dataIndex: 'F10', width: 90, style: 'text-align:left', align: 'right' },
            { text: "本月結存", dataIndex: 'F11', width: 90, style: 'text-align:left', align: 'right' },
            { text: "結存金額", dataIndex: 'F12', width: 90, style: 'text-align:left', align: 'right' },
            { text: "藥材類別", dataIndex: 'F13', width: 100, style: 'text-align:left', align: 'right' },
            { text: "付款方式", dataIndex: 'F14', width: 80, style: 'text-align:left', align: 'center' },
            { text: "合約方式", dataIndex: 'F15', width: 80, style: 'text-align:left', align: 'center' },
            { text: "是否戰備", dataIndex: 'F16', width: 80, style: 'text-align:left', align: 'center' },
            { text: "戰備存量", dataIndex: 'F17', width: 90, style: 'text-align:left', align: 'right' },
            { text: "戰備金額", dataIndex: 'F18', width: 90, style: 'text-align:left', align: 'right' },
            { text: "採購類別", dataIndex: 'F19', width: 80, style: 'text-align:left', align: 'center' },
            { text: "刪除品項", dataIndex: 'F20', width: 80, style: 'text-align:left', align: 'center' },
            { text: "特殊品項", dataIndex: 'F21', width: 80, style: 'text-align:left', align: 'center' },
            { text: "急救品項", dataIndex: 'F22', width: 80, style: 'text-align:left', align: 'center' },
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
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                items: [T1Grid]
            }
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P1=' + T1Query.getForm().findField('P1').rawValue
                + '&P2=' + T1Query.getForm().findField('P2').getValue()
                + '&P3=' + T1Query.getForm().findField('P3').getValue()
                + '&P4=' + T1Query.getForm().findField('P4').getValue()
                + '&P5=' + T1Query.getForm().findField('P5').getValue()
                + '&P6=' + T1Query.getForm().findField('P6').getValue()
                + '&P7=' + T1Query.getForm().findField('P7').getValue()
                + '&P8=' + T1Query.getForm().findField('P8').getValue()
                + '&P9=' + T1Query.getForm().findField('P9').getValue()
                + '&P10=' + T1Query.getForm().findField('P10').getValue()
                + '&P11=' + T1Query.getForm().findField('P11').getValue()
                + '&P12=' + T1Query.getForm().findField('P12').getValue()
                + '&P13=' + T1Query.getForm().findField('P13').getValue()
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
    
    //var objMask = new Ext.LoadMask({
    //    target: T2Form,
    //    msg: '讀取中...',
    //    hideMode: 'display',
    //    listeners: {
    //        beforedestroy: function (lmask) {
    //            //this.hide();
    //        },
    //        hide: function (lmask) {
    //            //this.hide();
    //        }
    //    }
    //});


});