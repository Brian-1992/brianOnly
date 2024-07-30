Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');

    var T1GetExcel1 = '../../../api/AA0174/Excel1';
    var T1GetExcel2 = '../../../api/AA0174/Excel2';

    var st_matclasssub = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: reqVal_g
            },
            url: '/api/AA0174/GetMatClassSubCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_warbak = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_e_sourcecode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_e_restrictcode = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_m_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var st_drugkind = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var inOoutBoundStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "in", "TEXT": "進貨" },
            { "VALUE": "out", "TEXT": "退貨" },
        ]
    });
    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AA0174/GetMatClassSubCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclasssubs = data.etts;
                    if (matclasssubs.length > 0) {
                        for (var i = 0; i < matclasssubs.length; i++) {
                            //st_matclasssub.add({ VALUE: matclasssubs[i].VALUE, TEXT: matclasssubs[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0174/GetWarBakCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var warbaks = data.etts;
                    if (warbaks.length > 0) {
                        for (var i = 0; i < warbaks.length; i++) {
                            st_warbak.add({ VALUE: warbaks[i].VALUE, TEXT: warbaks[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: '/api/AA0174/GetESourceCodeCombo',
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
            url: '/api/AA0174/GetERestrictcodeCombo',
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

        Ext.Ajax.request({
            url: '/api/AA0174/GetMContidCombo',
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
            url: '/api/AA0174/GetDrugKindCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var drugkinds = data.etts;
                    if (drugkinds.length > 0) {
                        for (var i = 0; i < drugkinds.length; i++) {
                            st_drugkind.add({ VALUE: drugkinds[i].VALUE, TEXT: drugkinds[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QueryPhVenderCombo = Ext.create('WEBAPP.form.PhVenderCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '廠商代碼/名稱',
        labelWidth: 100,
        width: T1QueryColWidthDefault,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0174/GetPHVenderCombo',
        //查詢完會回傳的欄位
        extraFields: ['AGEN_NO', 'EASYNAME', 'AGEN_NAMEC'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });
    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P3',
        name: 'P3',
        fieldLabel: '院內碼',
        labelWidth: 100,
        width: T1QueryColWidthDefault,
        emptyText: '全部',
        //allowBlank: true,
        //限制一次最多顯示10筆
        limit: 10,
        //指定查詢的Controller路徑
        queryUrl: '/api/AA0174/GetMMCodeCombo',
        //查詢完會回傳的欄位
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
        }
    });
    // 檢查必填欄位有沒有填值
    var T1CanQuery = function () {
        return (
            (T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P1').getValue() == "" || T1Query.getForm().findField('P1').getValue() == null)
        ) === false;
    };

    var T1QueryPanelHeight = 30;
    var T1QueryColWidth1 = 290;
    var T1QueryColWidth2 = 255;
    var T1QueryColWidth3 = 255;
    var T1QueryColWidthDefault = 200;

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
                    height: T1QueryPanelHeight,
                    items: [{
                        xtype: 'datefield',
                        fieldLabel: '查詢日期範圍',
                        name: 'P0',
                        id: 'P0',
                        fieldCls: 'required',
                        allowBlank: false, // 欄位為必填
                        value: new Date(),
                        labelWidth: 100,
                        width: 190
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '~',
                        name: 'P1',
                        id: 'P1',
                        labelSeparator: '',
                        fieldCls: 'required',
                        allowBlank: false, // 欄位為必填
                        value: new Date(),
                        labelWidth: 10,
                        width: 100
                    }, 
                        T1QueryPhVenderCombo, //P2
                        T1QuerymmCodeCombo //P3
                        , {
                        xtype: 'combo',
                        fieldLabel: '藥材類別',
                        name: 'P4',
                        id: 'P4',
                        labelWidth: 100,
                        width: T1QueryColWidthDefault,
                        emptyText: '全部',
                        store: st_matclasssub,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        matchFieldWidth: false,
                        listConfig: { width: 150 }
                    }, {
                        xtype: 'combo',
                        fieldLabel: '戰備',
                        name: 'P5',
                        id: 'P5',
                        labelWidth: 100,
                        width: T1QueryColWidthDefault,
                        emptyText: '全部',
                        store: st_warbak,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP2',
                    border: false,
                    layout: 'hbox',
                    height: T1QueryPanelHeight,
                    items: [{
                        xtype: 'combo',
                        fieldLabel: '買斷寄庫',
                        name: 'P6',
                        id: 'P6',
                        labelWidth: 100,
                        width: T1QueryColWidth1,
                        emptyText: '全部',
                        store: st_e_sourcecode,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '管制品',
                        name: 'P7',
                        id: 'P7',
                        labelWidth: 100,
                        width: T1QueryColWidth2,
                        emptyText: '全部',
                        store: st_e_restrictcode,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '進出貨',
                        name: 'P8',
                        id: 'P8',
                        labelWidth: 100,
                        width: T1QueryColWidth3,
                        emptyText: '全部',
                        store: inOoutBoundStore,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '是否合約',
                        name: 'P10',
                        id: 'P10',
                        labelWidth: 100,
                        width: T1QueryColWidthDefault,
                        emptyText: '全部',
                        store: st_m_contid,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }
                        , {
                        xtype: 'textfield',
                        fieldLabel: '訂單號碼',
                        name: 'P11',
                        id: 'P11',
                        labelWidth: 100,
                        width: T1QueryColWidthDefault
                    }]
                }, 
                    {
                    xtype: 'panel',
                    id: 'PanelP3',
                    border: false,
                    layout: 'hbox',
                    items: [{
                        xtype: 'textfield',
                        fieldLabel: '發票',
                        name: 'P13',
                        id: 'P13',
                        labelWidth: 100,
                        width: T1QueryColWidth1
                    }, {
                        xtype: 'combo',
                        fieldLabel: '中西藥類別',
                        name: 'P12',
                        id: 'P12',
                        labelWidth: 110,
                        width: T1QueryColWidth2,
                        emptyText: '全部',
                        store: st_drugkind,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE'
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '批號',
                        name: 'P14',
                        id: 'P14',
                        labelWidth: 100,
                        width: T1QueryColWidth3
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '效期',
                        name: 'P15',
                        id: 'P15',
                        labelWidth: 100,
                        width: T1QueryColWidthDefault
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '~',
                        name: 'P16',
                        id: 'P16',
                        labelSeparator: '',
                        labelWidth: 10,
                        width: 100
                    },
                    {
                        xtype: 'tbspacer',
                        width: 20
                    },
                    {
                        xtype: 'button',
                        text: '查詢',
                        id: 'T1btn1',
                        handler: function () {
                            if (!T1CanQuery()) {
                                Ext.Msg.alert('訊息', '需填查詢日期範圍');
                                return;
                            }
                            T1Load();
                        }
                    },
                    {
                        xtype: 'tbspacer',
                        width: 10
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }]
                },
                //{
                //    xtype: 'panel',
                //    id: 'PanelP4',
                //    border: false,
                //    layout: 'hbox',
                //    items: [{
                //        xtype: 'textfield',
                //        fieldLabel: '發票',
                //        name: 'P13',
                //        id: 'P13',
                //        labelWidth: 100,
                //        width: 290
                //    }, {
                //        xtype: 'combo',
                //        fieldLabel: '中西藥類別',
                //        name: 'P12',
                //        id: 'P12',
                //        labelWidth: 110,
                //        width: 310,
                //        emptyText: '全部',
                //        store: st_drugkind,
                //        queryMode: 'local',
                //        displayField: 'TEXT',
                //        valueField: 'VALUE'
                //    }, {
                //        xtype: 'textfield',
                //        fieldLabel: '批號',
                //        name: 'P14',
                //        id: 'P14',
                //        labelWidth: 100,
                //        width: 310
                //    }, {
                //        xtype: 'datefield',
                //        fieldLabel: '效期',
                //        name: 'P15',
                //        id: 'P15',
                //        labelWidth: 100,
                //        width: 195
                //    }, {
                //        xtype: 'datefield',
                //        fieldLabel: '~',
                //        name: 'P16',
                //        id: 'P16',
                //        labelSeparator: '',
                //        labelWidth: 10,
                //        width: 105
                //    }, {
                //        xtype: 'tbspacer',
                //        width: 20
                //    }, {
                //        xtype: 'button',
                //        text: '查詢',
                //        id: 'T1btn1',
                //        handler: function () {
                //            if (!T1CanQuery()) {
                //                Ext.Msg.alert('訊息', '需填查詢日期範圍');
                //                return;
                //            }
                //            T1Load();
                //        }
                //    }, {
                //        xtype: 'button',
                //        text: '清除',
                //        handler: function () {
                //            var f = this.up('form').getForm();
                //            f.reset();
                //            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                //        }
                //    }]
                //}

                ]
            }
        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'F1', type: 'string' },
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
            { name: 'F24', type: 'string' },
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
            { name: 'F40', type: 'string' },
            { name: 'F41', type: 'string' },
            { name: 'F42', type: 'string' },
            { name: 'F43', type: 'string' },
            { name: 'F44', type: 'string' },
            { name: 'F45', type: 'string' },
            { name: 'F46', type: 'string' },
            { name: 'F47', type: 'string' },
            { name: 'F48', type: 'string' },
            { name: 'F49', type: 'string' },
            { name: 'F50', type: 'string' },
            { name: 'F51', type: 'string' }
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
            url: '/api/AA0174/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
        },
        listeners: {
            load: function (store, options) {   //設定匯出是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    T1Tool.down('#export1').setDisabled(false);
                    T1Tool.down('#export2').setDisabled(false);
                } else {
                    T1Tool.down('#export1').setDisabled(true);
                    T1Tool.down('#export2').setDisabled(true);;
                }

                // 合計
                var T2FieldIdList = [
                    'F1', 'F2', 'F3', 'F4', 'F5',
                    'F6', 'F7', 'F8', 'F9', 'F10'];

                T2FieldIdList.forEach(function (val) {
                    var funcUrl = '/api/AA0174/GetT2' + val,
                        fieldId = val;

                    Ext.Ajax.request({
                        url: funcUrl,
                        params: {
                            p0: T1Query.getForm().findField('P0').rawValue,
                            p1: T1Query.getForm().findField('P1').rawValue,
                            p2: T1Query.getForm().findField('P2').getValue(),
                            p3: T1Query.getForm().findField('P3').getValue(),
                            p4: T1Query.getForm().findField('P4').getValue(),
                            p5: T1Query.getForm().findField('P5').getValue(),
                            p6: T1Query.getForm().findField('P6').getValue(),
                            p7: T1Query.getForm().findField('P7').getValue(),
                            p8: T1Query.getForm().findField('P8').getValue(),
                            p10: T1Query.getForm().findField('P10').getValue(),
                            p11: T1Query.getForm().findField('P11').getValue(),
                            p12: T1Query.getForm().findField('P12').getValue(),
                            p13: T1Query.getForm().findField('P13').getValue(),
                            p14: T1Query.getForm().findField('P14').getValue(),
                            p15: T1Query.getForm().findField('P15').rawValue,
                            p16: T1Query.getForm().findField('P16').rawValue

                        },
                        method: reqVal_p,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T2Form.getForm().findField(fieldId).setValue(data.msg);
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                })
                return;
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
        T1Store.getProxy().setExtraParam("p10", T1Query.getForm().findField('P10').getValue());
        T1Store.getProxy().setExtraParam("p11", T1Query.getForm().findField('P11').getValue());
        T1Store.getProxy().setExtraParam("p12", T1Query.getForm().findField('P12').getValue());
        T1Store.getProxy().setExtraParam("p13", T1Query.getForm().findField('P13').getValue());
        T1Store.getProxy().setExtraParam("p14", T1Query.getForm().findField('P14').getValue());
        T1Store.getProxy().setExtraParam("p15", T1Query.getForm().findField('P15').rawValue);
        T1Store.getProxy().setExtraParam("p16", T1Query.getForm().findField('P16').rawValue);
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
                itemId: 'export1', text: '匯出申報健保廠商', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '申報健保廠商' });
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
                    p.push({ name: 'p13', value: T1Query.getForm().findField('P13').getValue() });
                    p.push({ name: 'p14', value: T1Query.getForm().findField('P14').getValue() });
                    p.push({ name: 'p15', value: T1Query.getForm().findField('P15').rawValue });
                    p.push({ name: 'p16', value: T1Query.getForm().findField('P16').rawValue });
                    PostForm(T1GetExcel1, p);
                    msglabel('訊息區:匯出完成');
                }
            }, {
                itemId: 'export2', text: '匯出Excel', disabled: true,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '廠商進退貨明細' });
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
                    p.push({ name: 'p13', value: T1Query.getForm().findField('P13').getValue() });
                    p.push({ name: 'p14', value: T1Query.getForm().findField('P14').getValue() });
                    p.push({ name: 'p15', value: T1Query.getForm().findField('P15').rawValue });
                    p.push({ name: 'p16', value: T1Query.getForm().findField('P16').rawValue });

                    PostForm(T1GetExcel2, p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                id: 'print',
                name: 'print',
                text: '列印',
                handler: function () {
                    if (T1Store.getCount() > 0) {
                        //msglabel(T1Query.getForm().findField('D0').getValue() + "," + T1Query.getForm().findField('D1').getValue());

                        var reportUrl = '/Report/A/AA0174.aspx';
                        if (!win) {
                            var winform = Ext.create('Ext.form.Panel', {
                                id: 'iframeReport',
                                //height: '100%',
                                //width: '100%',
                                layout: 'fit',
                                closable: false,
                                html: '<iframe src="' + reportUrl + '?type=' + "AA0174"
                                + '&p0=' + T1Query.getForm().findField('P0').rawValue
                                + '&p1=' + T1Query.getForm().findField('P1').rawValue
                                + '&p2=' + T1Query.getForm().findField('P2').getValue()
                                + '&p3=' + T1Query.getForm().findField('P3').getValue()
                                + '&p4=' + T1Query.getForm().findField('P4').getValue()
                                + '&p5=' + T1Query.getForm().findField('P5').getValue()
                                + '&p6=' + T1Query.getForm().findField('P6').getValue()
                                +' &p7='+ T1Query.getForm().findField('P7').getValue()
                                + '&p8='+ T1Query.getForm().findField('P8').getValue()
                                + '&p10=' + T1Query.getForm().findField('P10').getValue()
                                + '&p11=' + T1Query.getForm().findField('P11').getValue()
                                + '&p12=' + T1Query.getForm().findField('P12').getValue()
                                + '&p13=' + T1Query.getForm().findField('P13').getValue()
                                + '&p14=' + T1Query.getForm().findField('P14').getValue()
                                + '&p15=' + T1Query.getForm().findField('P15').rawValue
                                + '&p16=' + T1Query.getForm().findField('P16').rawValue
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
                    else
                        Ext.Msg.alert('訊息', '請先建立資料');
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
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            { text: "類別", dataIndex: 'F1', width: 60 },
            { text: "日期", dataIndex: 'F3', width: 90 },
            { text: "訂單號碼", dataIndex: 'F4', width: 110 },
            { text: "廠商代碼", dataIndex: 'F5', width: 90 },
            { text: "廠商統編", dataIndex: 'F6', width: 90 },
            { text: "廠商名稱", dataIndex: 'F7', width: 100 },
            { text: "藥材代碼", dataIndex: 'F8', width: 100 },
            { text: "藥材名稱(中+英)", dataIndex: 'F34', width: 200 },
            { text: "包裝", dataIndex: 'F9', width: 60 },
            { text: "單位", dataIndex: 'F10', width: 60 },
            { text: "數量", dataIndex: 'F11', width: 60, style: 'text-align:left', align: 'right' },
            { text: "末效期", dataIndex: 'F12', width: 80 },
            { text: "單價", dataIndex: 'F13', width: 90, style: 'text-align:left', align: 'right' },
            { text: "贈品數量", dataIndex: 'F14', width: 90, style: 'text-align:left', align: 'right' },
            { text: "折讓金額", dataIndex: 'F15', width: 90, style: 'text-align:left', align: 'right' },
            { text: "小計", dataIndex: 'F16', width: 100, style: 'text-align:left', align: 'right' },
            { text: "發票日期", dataIndex: 'F19', width: 100 },
            { text: "發票號碼", dataIndex: 'F20', width: 100 },
            { text: "備註", dataIndex: 'F21', width: 100 },
            { text: "贈品小計", dataIndex: 'F22', width: 100, style: 'text-align:left', align: 'right' },
            { text: "交貨日期", dataIndex: 'F23', width: 100 },
            { text: "製造批號", dataIndex: 'F24', width: 100 },
            { text: "病患病歷號", dataIndex: 'F27', width: 100 },
            { text: "病患姓名", dataIndex: 'F28', width: 100 },
            { text: "明細備註", dataIndex: 'F29', width: 100 },
            { text: "廠商地址", dataIndex: 'F30', width: 200 },
            { text: "合約優惠金額", dataIndex: 'F31', width: 140, style: 'text-align:left', align: 'right' },
            { text: "調整後小計金額", dataIndex: 'F32', width: 160, style: 'text-align:left', align: 'right' },
            { text: "調整後優惠金額", dataIndex: 'F33', width: 160, style: 'text-align:left', align: 'right' },
            { text: "管制品", dataIndex: 'F35', width: 100 },
            { text: "買斷寄庫", dataIndex: 'F36', width: 100 },
            { text: "案號", dataIndex: 'F37', width: 100 },
            { text: "合約到期日", dataIndex: 'F38', width: 100 },
            { text: "合約類別", dataIndex: 'F39', width: 100 },
            { text: "是否合約", dataIndex: 'F40', width: 100 },
            { text: "健保代碼", dataIndex: 'F41', width: 100 },
            { text: "健保價", dataIndex: 'F42', width: 100, style: 'text-align:left', align: 'right' },
            { text: "合約價", dataIndex: 'F43', width: 100, style: 'text-align:left', align: 'right' },
            { text: "合約成本差額", dataIndex: 'F44', width: 140, style: 'text-align:left', align: 'right' },
            { text: "合約小計", dataIndex: 'F45', width: 100, style: 'text-align:left', align: 'right' },
            { text: "合約贈品小計", dataIndex: 'F46', width: 140, style: 'text-align:left', align: 'right' },
            { text: "許可證號", dataIndex: 'F47', width: 100 },
            { text: "聯標項次", dataIndex: 'F48', width: 100 },
            { text: "中西藥類別", dataIndex: 'F49', width: 100 },
            { text: "優惠比", dataIndex: 'F50', width: 100, style: 'text-align:left', align: 'right' },
            { text: "優惠單價", dataIndex: 'F51', width: 100, style: 'text-align:left', align: 'right' },
            { header: "", flex: 1 }
        ],
        listeners: {
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
            { name: 'F10', type: 'string' }
        ]

    });


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

            { fieldLabel: '進貨合計', name: 'F1', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '退貨合計', name: 'F2', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '贈品合計', name: 'F3', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '總計金額', name: 'F4', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },

            { fieldLabel: '合約進貨合計', name: 'F5', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '合約退貨合計', name: 'F6', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '合約贈品合計', name: 'F7', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },
            { fieldLabel: '合約總計金額', name: 'F8', readOnly: true, width: mWidth, xtype: 'displayfield', align: 'right' },

            { fieldLabel: '折讓合計', name: 'F9', readOnly: true, width: mWidth, colspan: 3, xtype: 'displayfield' },
            { fieldLabel: '總計合計成本差', name: 'F10', readOnly: true, width: mWidth, xtype: 'displayfield' },

            { xtype: 'label', colspan: 4, style: 'color: #ff0000;', text: '請注意:各品項成本單價比合約價高時，會產生負數合約成本差，<總計合約成本差>可能變少，另非合約不計入差額' },
            { xtype: 'label', colspan: 4, style: 'color: #ff0000;', text: '　　　<總計金額>及<合約總計金額>皆已扣除[折讓金額](折讓金額是指進貨單額外輸入的折讓金額)' },
            { xtype: 'label', colspan: 4, style: 'color: #ff0000;', text: '　　　另外，資料中若有[退貨項目]，<總計金額>及<合約總計金額>也會扣除退貨金額' }

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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            height: '75%',
            items: [T1Grid]
        },
        {
            region: 'south',
            layout: 'fit',
            collapsible: false,
            title: '',
            height: '25%',
            split: true,
            items: [T2Form]
        }
        ]
    });

    function MenuLinkSet() {
        if (menuLink == "BG0012") {
            T1Query.getForm().findField('P4').setValue("all02");
        }
    }

    T1Query.getForm().findField('P0').focus();
    MenuLinkSet();
});