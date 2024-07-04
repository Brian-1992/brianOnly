Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T2Set = '';
    var MmcodeComboGet = '/api/AA0158/GetMMCodeCombo';
    var MatclassComboGet = '/api/AA0158/GetMatclassCombo';
    var MatclassSubComboGet = '/api/AA0158/GetMatclassSubCombo';
    var MatclassSubFComboGet = '/api/AA0158/GetMatclassSubFCombo';
    var BaseunitComboGet = '/api/AA0158/GetBaseunitCombo';
    var YonNComboGet = '/api/AA0158/GetYnCombo';
    var AgennoComboGet = '/api/AA0158/GetAgennoCombo';
    var ESourceCodeComboGet = '/api/AA0158/GetESourceCodeCombo';
    var ERestrictCodeComboGet = '/api/AA0158/GetERestrictCodeCombo';
    var WarbakComboGet = '/api/AA0158/GetWarbakCombo';
    var OnecostComboGet = '/api/AA0158/GetOnecostCombo';
    var HealthPayComboGet = '/api/AA0158/GetHealthPayCombo';
    var CostKindComboGet = '/api/AA0158/GetCostKindCombo';
    var WastKindComboGet = '/api/AA0158/GetWastKindCombo';
    var SpXfeeComboGet = '/api/AA0158/GetSpXfeeCombo';
    var OrderKindComboGet = '/api/AA0158/GetOrderKindCombo';
    var DrugKindComboGet = '/api/AA0158/GetDrugKindCombo';
    var SpDrugComboGet = '/api/AA0158/GetSpDrugCombo';
    var FastDrugComboGet = '/api/AA0158/GetFastDrugCombo';
    var MContidComboGet = '/api/AA0158/GetMContidCombo';
    var TouchCaseComboGet = '/api/AA0158/GetTouchCaseCombo';
    var CommonComboGet = '/api/AA0158/GetCommonCombo';
    var MStoreidComboGet = '/api/AA0158/MStoreidComboGet';
    var AgenNamecComboGet = '/api/AA0158/GetAgenNamecCombo';
    var UniNoComboGet = '/api/AA0158/GetUniNoCombo';
    var MPhctncoComboGet = '/api/AA0158/GetMPhctncoCombo';
    var CaseNoComboGet = '/api/AA0158/GetCaseNoCombo';
    var T1Name = "藥衛材基本檔維護作業";
    var T1GetExcel = '/api/AA0158/Excel';

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;
    var selectedMmcode = null;

    // 不卡合約優惠 10/19/2023
    var calDiscPrice = function () {
        if ((T2Form.getForm().findField("M_CONTPRICE").getValue() !== null && T2Form.getForm().findField("M_CONTPRICE").getValue() > 0 && T2Form.getForm().findField("M_CONTPRICE").getValue() !== '') &&
            (T2Form.getForm().findField("DISC_CPRICE").getValue() !== null && T2Form.getForm().findField("DISC_CPRICE").getValue() > 0 && T2Form.getForm().findField("DISC_CPRICE").getValue() !== '') &&
            (T2Form.getForm().findField("M_CONTPRICE").getValue() > T2Form.getForm().findField("DISC_CPRICE").getValue())
        ) {
            var m_contprice = T2Form.getForm().findField("M_CONTPRICE").getValue();
            var disc_cprice = T2Form.getForm().findField("DISC_CPRICE").getValue();
            T2Form.getForm().findField("DISC_RATIO").setValue(Math.ceil(disc_cprice / m_contprice * 100) + "%");
        } else {
            T2Form.getForm().findField("DISC_RATIO").setValue("");
        }
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isAB = Ext.getUrlParam('isAB') == 'Y';
    var menuLink = Ext.getUrlParam('menuLink');

    var matclassFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var matclassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    var matclassSubStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: MatclassSubComboGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //var matclassSubStore = Ext.create('Ext.data.Store', {
    //    fields: ['VALUE', 'TEXT', 'COMBITEM']
    //});
    var matclassSubFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var st_cancelid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var st_dcflag = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var form_YN = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "", "TEXT": "", "COMBITEM": "" },
            { "VALUE": "Y", "TEXT": "Y", "COMBITEM": "Y 作廢" },
            { "VALUE": "N", "TEXT": "N", "COMBITEM": "N 使用" }
        ]
    });

    var form_YN2 = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "Y", "TEXT": "Y", "COMBITEM": "Y" },
            { "VALUE": "N", "TEXT": "N", "COMBITEM": "N" }
        ]
    });

    var baseunitFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    // 來源代碼代碼
    var eSourceCodeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    var eRestrictCodeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var WarbakStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var OnecostStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var HealthPayStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var CostKindStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var WastKindStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var SpXfeeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var OrderKindStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var DrugKindStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var SpDrugStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var FastDrugStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var MContidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var TouchCaseStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var CommonStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var IsivStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var MStoreidStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,

            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassFormStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: MatclassSubFComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassSubFormStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        
        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,

            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: MatclassSubComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            //matclassSubStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }

                        if (menuLink == 'AB0143') {
                            T1Query.getForm().findField('P1').setValue('02');
                        }
                        else if (menuLink == 'AB0144') {
                            T1Query.getForm().findField('P1').setValue('01');
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: YonNComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        st_cancelid.add({ VALUE: '', TEXT: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            st_cancelid.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: YonNComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        st_dcflag.add({ VALUE: '', TEXT: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            st_dcflag.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: BaseunitComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            baseunitFormStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: ESourceCodeComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            eSourceCodeStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: ERestrictCodeComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            eRestrictCodeStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: WarbakComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            WarbakStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: OnecostComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            OnecostStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: HealthPayComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            HealthPayStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: CostKindComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            CostKindStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: WastKindComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            WastKindStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: SpXfeeComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            SpXfeeStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: OrderKindComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            OrderKindStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: DrugKindComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            DrugKindStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: SpDrugComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            SpDrugStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: FastDrugComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            FastDrugStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: MContidComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            MContidStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: TouchCaseComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            TouchCaseStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
        Ext.Ajax.request({
            url: CommonComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            CommonStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: YonNComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        IsivStore.add({ VALUE: '', TEXT: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            IsivStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: MStoreidComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            MStoreidStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
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
        id: 'P2',
        name: 'P2',
        fieldLabel: '院內碼',
        emptyText: '全部',
        width: 200,
        limit: 10,
        queryUrl: MmcodeComboGet,
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        getDefaultParams: function () {

        },
        listeners: {
        }
    });

    var T2FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'M_AGENNO',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: AgennoComboGet,
        //storeAutoLoad: true,
        matchFieldWidth: false,
        listConfig: { width: 300 },
        readOnly: true,
        colspan: 1,
        listeners: {
            /*blur: function (field, eve, eOpts) {
                if (field.getValue() != '' && field.getValue() != null)
                    //chkAGENNO(field.getValue());
            },*/
            select: function (c, r, i, e) {
                var f = T2Form.getForm();
                if (r.get('AGEN_NAMEC') != '' && r.get('AGEN_NAMEC') != null) {
                    f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEC'));
                }
                else if (r.get('AGEN_NAMEE') != '' && r.get('AGEN_NAMEE') != null) {
                    f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEE'));
                }
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 100;
    var mWidth = 300;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'panel',
            id: 'PanelT1P1',
            border: false,
            layout: 'vbox',
            autoScroll: true,
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '品別',
                    name: 'P0',
                    id: 'P0',
                    width: 200,
                    emptyText: '全部',
                    store: CommonStore,
                    queryMode: 'local',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE'
                }, {
                    xtype: 'combo',
                    fieldLabel: '類別',
                    name: 'P1',
                    id: 'P1',
                    width: 200,
                    emptyText: '全部',
                    store: matclassSubStore,
                    queryMode: 'local',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE'
                },
                T1QuerymmCodeCombo, //P2

                Ext.create('WEBAPP.form.QueryCombo', {
                    fieldLabel: '廠商名稱',
                    queryUrl: '/api/AA0158/GetAgenNamecCombo',
                    extraFields: ['EXTRA1'],
                    id: 'P3',
                    name: 'P3',
                    enforceMaxLength: true,
                    maxLength: 30,
                    width: 200,
                    emptyText: '全部',
                    matchFieldWidth: false,
                    valueField: 'TEXT',
                    displayField: 'COMBITEM',
                    listConfig: {
                        resizable: true
                    }
                }),
                Ext.create('WEBAPP.form.QueryCombo', {
                    fieldLabel: '統編',
                    queryUrl: '/api/AA0158/GetUniNoCombo',
                    extraFields: ['EXTRA1'],
                    id: 'P4',
                    name: 'P4',
                    enforceMaxLength: true,
                    maxLength: 30,
                    width: 200,
                    emptyText: '全部',
                    displayField: 'VALUE'
                }),
                Ext.create('WEBAPP.form.QueryCombo', {
                    fieldLabel: '許可證號',
                    queryUrl: '/api/AA0158/GetMPhctncoCombo',
                    extraFields: ['EXTRA1'],
                    id: 'P5',
                    name: 'P5',
                    enforceMaxLength: true,
                    maxLength: 30,
                    width: 200,
                    emptyText: '全部',
                    matchFieldWidth: false,
                    listConfig: {
                        resizable: true,
                        displayField: 'VALUE'
                    }
                }),
                Ext.create('WEBAPP.form.QueryCombo', {
                    fieldLabel: '合約案號',
                    queryUrl: '/api/AA0158/GetCaseNoCombo',
                    extraFields: ['EXTRA1'],
                    id: 'P6',
                    name: 'P6',
                    enforceMaxLength: true,
                    maxLength: 30,
                    width: 200,
                    emptyText: '全部',
                    displayField: 'VALUE'
                }),
                Ext.create('WEBAPP.form.QueryCombo', {
                    fieldLabel: '中西藥分類',
                    queryUrl: '/api/AA0158/GetDrugKindCombo',
                    extraFields: ['EXTRA1'],
                    id: 'P7',
                    name: 'P7',
                    enforceMaxLength: true,
                    maxLength: 30,
                    width: 200,
                    emptyText: '全部',
                    displayField: 'TEXT'
                }),
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    autoScroll: true,
                    items: [
                        {
                            xtype: 'button',
                            id: 'btnQuery',
                            itemId: 'query', text: '查詢',
                            handler: function () {
                                T1Load(true, true);
                                msglabel('訊息區:');
                            }
                        }, {
                            xtype: 'button',
                            id: 'btnClean',
                            itemId: 'clean', text: '清除', handler: function () {
                                var f = T1Query.getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                                msglabel('訊息區:');
                            }
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    autoScroll: true,
                    items: [
                        {
                            xtype: 'button',
                            id: 'btnAdd',
                            itemId: 'add', text: '新增', hidden: (isAB == true),
                            handler: function () {
                                msglabel('訊息區:');
                                T1Set = '/api/AA0158/Create';
                                setFormT1('I', '新增');
                            }
                        }, {
                            xtype: 'button',
                            id: 'btnEdit',
                            disabled: true, hidden: (isAB == true),
                            itemId: 'edit', text: '修改', handler: function () {
                                msglabel('訊息區:');
                                T1Set = '/api/AA0158/Update';
                                setFormT1("U", '修改');
                            }
                        }, {
                            xtype: 'button',
                            id: 'btnExport',
                            disabled: true, 
                            itemId: 'export', text: '匯出', handler: function () {

                                var p = new Array();
                                p.push({ name: 'FN', value: '藥衛材基本檔維護作業.xlsx' });
                                p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                                p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                                p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                                p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                                p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                                p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                                p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                                p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() });
                                PostForm(T1GetExcel, p);
                                msglabel('訊息區:匯出完成');
                            }
                        },
                        {
                            xtype: 'button',
                            itemId: 'import',
                            id: 'btnIxport',
                            text: '匯入', hidden: (isAB == true),
                            handler: function () {
                                msglabel('訊息區:');
                                showWin6();
                            }
                        },
                        {
                            xtype: 'button',
                            id: 'btnCopy',
                            disabled: true, hidden: (isAB == true),
                            itemId: 'copy', text: '複製轉新增', handler: function () {
                                msglabel('訊息區:');
                                showWin3();
                            }
                        }
                    ]
                }
            ]
        }
        ]
    });

    function setT2Form() {
        T2Form.getForm().findField('MMCODE').setValue(T11Form.getForm().findField('MMCODE').getValue());
        T2Form.getForm().findField('M_NHIKEY').setValue(T11Form.getForm().findField('M_NHIKEY').getValue());
        T2Form.getForm().findField('HEALTHOWNEXP').setValue(T11Form.getForm().findField('HEALTHOWNEXP').getValue());
        T2Form.getForm().findField('DRUGSNAME').setValue(T11Form.getForm().findField('DRUGSNAME').getValue());
        T2Form.getForm().findField('MMNAME_E').setValue(T11Form.getForm().findField('MMNAME_E').getValue());
        T2Form.getForm().findField('MMNAME_C').setValue(T11Form.getForm().findField('MMNAME_C').getValue());
        T2Form.getForm().findField('M_PHCTNCO').setValue(T11Form.getForm().findField('M_PHCTNCO').getValue());
        T2Form.getForm().findField('M_ENVDT').setValue(T11Form.getForm().findField('M_ENVDT').getValue());
        T2Form.getForm().findField('ISSUESUPPLY').setValue(T11Form.getForm().findField('ISSUESUPPLY').getValue());
        T2Form.getForm().findField('E_MANUFACT').setValue(T11Form.getForm().findField('E_MANUFACT').getValue());
        T2Form.getForm().findField('BASE_UNIT').setValue(T12Form.getForm().findField('BASE_UNIT').getValue());
        T2Form.getForm().findField('M_PURUN').setValue(T12Form.getForm().findField('M_PURUN').getValue());
        T2Form.getForm().findField('UNITRATE').setValue(T12Form.getForm().findField('UNITRATE').getValue());
        T2Form.getForm().findField('MAT_CLASS_SUB').setValue(T12Form.getForm().findField('MAT_CLASS_SUB').getValue());
        T2Form.getForm().findField('COMMON').setValue(T12Form.getForm().findField('COMMON').getValue());
        T2Form.getForm().findField('TRUTRATE').setValue(T12Form.getForm().findField('TRUTRATE').getValue());
        T2Form.getForm().findField('ONECOST').setValue(T12Form.getForm().findField('ONECOST').getValue());
        T2Form.getForm().findField('HEALTHPAY').setValue(T12Form.getForm().findField('HEALTHPAY').getValue());
        T2Form.getForm().findField('COSTKIND').setValue(T12Form.getForm().findField('COSTKIND').getValue());
        T2Form.getForm().findField('E_RESTRICTCODE').setValue(T12Form.getForm().findField('E_RESTRICTCODE').getValue());
        T2Form.getForm().findField('WARBAK').setValue(T12Form.getForm().findField('WARBAK').getValue());
        T2Form.getForm().findField('WASTKIND').setValue(T12Form.getForm().findField('WASTKIND').getValue());
        T2Form.getForm().findField('SPXFEE').setValue(T12Form.getForm().findField('SPXFEE').getValue());
        T2Form.getForm().findField('ORDERKIND').setValue(T12Form.getForm().findField('ORDERKIND').getValue());
        T2Form.getForm().findField('CASEDOCT').setValue(T12Form.getForm().findField('CASEDOCT').getValue());
        T2Form.getForm().findField('CANCEL_ID').setValue(T13Form.getForm().findField('CANCEL_ID').getValue());
        T2Form.getForm().findField('M_STOREID').setValue(T13Form.getForm().findField('M_STOREID').getValue());
        //T2Form.getForm().findField('E_ORDERDCFLAG').setValue(T13Form.getForm().findField('E_ORDERDCFLAG').getValue());
        T2Form.getForm().findField('DRUGKIND').setValue(T13Form.getForm().findField('DRUGKIND').getValue());
        T2Form.getForm().findField('SPDRUG').setValue(T13Form.getForm().findField('SPDRUG').getValue());
        T2Form.getForm().findField('SPMMCODE').setValue(T13Form.getForm().findField('SPMMCODE').getValue());
        T2Form.getForm().findField('FASTDRUG').setValue(T13Form.getForm().findField('FASTDRUG').getValue());
        T2Form.getForm().findField('MIMASTHIS_SEQ').setValue(T13Form.getForm().findField('MIMASTHIS_SEQ').getValue());
        T2Form.getForm().findField('APPQTY_TIMES').setValue(T13Form.getForm().findField('APPQTY_TIMES').getValue());
        T2Form.getForm().findField('ISIV').setValue(T13Form.getForm().findField('ISIV').getValue());
        T2Form.getForm().findField('E_RETURNDRUGFLAG').setValue(T13Form.getForm().findField('E_RETURNDRUGFLAG').getValue());
        T2Form.getForm().findField('E_VACCINE').setValue(T13Form.getForm().findField('E_VACCINE').getValue());

        // T2Grid 的 健保價，決標價，成本價，聯標項次契約總價等價格欄位會影響 money_chagne 是否要秀在 AA0158
        if (T2Form.isDirty()) {
            var money_change = 'N',
                dirtyValues = T2Form.getValues(false, true);
            if (Object.hasOwn(dirtyValues, 'DISC_CPRICE') ||
                Object.hasOwn(dirtyValues, 'M_CONTPRICE') ||
                Object.hasOwn(dirtyValues, 'NHI_PRICE') ||
                Object.hasOwn(dirtyValues, 'CONTRACTSUM')) {
                money_change = 'Y';
            }
            T2Form.getForm().findField('MONEYCHANGE').setValue(money_change);
        }
    }

    function T1Cleanup() {
        var forms = [T11Form, T12Form, T13Form, T2Form];

        forms.forEach(function (el) {
            el.getForm().reset();
            el.getForm().getFields().each(function (fc) {
                fc.setReadOnly(true);
                //fc.readOnly = true;
            });
        })

        Ext.getCmp('btnSave').setDisabled(true);
        Ext.getCmp('btnCancel').setDisabled(true);
    }

    var T1Store = Ext.create('WEBAPP.store.AA.AA0158VM', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (menuLink == "AA0199" || menuLink == "AA0204") {
                } else {
                    Ext.getCmp('btnExport').setDisabled(records.length == 0);
                }
                if (selectedMmcode != null) {
                    var index = T1Grid.getStore().find('MMCODE', selectedMmcode);
                    if (index >= 0) {
                        T1Grid.getSelectionModel().select(index);
                    }

                }
            }
        }
    });
    function T1Load(selectedMmcodeNull, loadFirst) {
        if (loadFirst) {
            T1Tool.moveFirst();
        } else {
            T1Store.load({
                params: {
                    start: 0
                }
            });
        }


        if (selectedMmcodeNull) {
            selectedMmcode = null;
        }
        MenuLinkSet();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
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
            { text: "院內碼", dataIndex: 'MMCODE', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec != null) {
                    selectedMmcode = T1LastRec.data.MMCODE;
                }

                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        Ext.getCmp('btnEdit').setDisabled(T1Rec === 0);
        if (menuLink == "AA0199" || menuLink == "AA0204") {
        } else {
            Ext.getCmp('btnCopy').setDisabled(T1Rec === 0);
        }
        if (T1LastRec) {
            isNew = false;
            T11Form.loadRecord(T1LastRec);
            T12Form.loadRecord(T1LastRec);
            T13Form.loadRecord(T1LastRec);
            T2Form.loadRecord(T1LastRec);
            calDiscPrice();
            T2Load();
            var f = T11Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T11Form.getForm().reset();
            T12Form.getForm().reset();
            T13Form.getForm().reset();
            T2Form.getForm().reset();
        }
    }

    function setFormT1(x, t) {
        TATabs.setActiveTab('Tab1');
        var f = T11Form.getForm();
        if (x === "I") {
            isNew = true;
            T1LastRec = "";
            T1Cleanup();
            T1Store.removeAll();
            T2Store.removeAll();
            T1Query.getForm().reset();
            var r = Ext.create('WEBAPP.model.MiMast');
            T11Form.loadRecord(r);
            T12Form.loadRecord(r);
            T13Form.loadRecord(r);
            T2Form.loadRecord(r);
            u = f.findField("MMCODE");
            u.setReadOnly(false);
            f.defaultFocus = u;
            T12Form.getForm().findField('E_RESTRICTCODE').setValue('N');
            T12Form.getForm().findField('TRUTRATE').setValue('1');
            T12Form.getForm().findField('WARBAK').setValue('0');
            T12Form.getForm().findField('ONECOST').setValue('0');
            T13Form.getForm().findField('CANCEL_ID').setValue('N'); 
            T13Form.getForm().findField('ISIV').setValue('N');
            T13Form.getForm().findField('E_RETURNDRUGFLAG').setValue('N');
            T13Form.getForm().findField('E_VACCINE').setValue('N');
            //T13Form.getForm().findField('DISCOUNT_QTY').setValue('1');
            T13Form.getForm().findField('APPQTY_TIMES').setValue('1');
            T13Form.getForm().findField('M_STOREID').setValue('0');
            T2Form.getForm().findField('E_SOURCECODE').setValue('N');
            T2Form.getForm().findField('M_CONTID').setValue('0');
            //T2Form.getForm().findField('CONTRACTAMT').setValue('0');
            //T2Form.getForm().findField('CONTRACTSUM').setValue('0');
            T2Form.getForm().findField('TOUCHCASE').setValue('0');
            Ext.getCmp('btnSearch').setDisabled(false);
        }
        else {

            u = f.findField('MMCODE');
            u.setReadOnly(true);
        }
        f.defaultFocus = u;
        T11Form.getForm().findField('x').setValue(x);
        T12Form.getForm().findField('x').setValue(x);
        T13Form.getForm().findField('x').setValue(x);
        T2Form.getForm().findField('x').setValue(x);
        MenuLinkSet();
        if (menuLink == "AA0199") {
            T12Form.getForm().findField('M_PURUN').setReadOnly(false);
            T12Form.getForm().findField('UNITRATE').setReadOnly(false);
        }
        else if (menuLink == "AA0204") {
            T2Form.getForm().findField('E_ITEMARMYNO').setReadOnly(false);
            T2Form.getForm().findField('CONTRACTAMT').setReadOnly(false);
            T2Form.getForm().findField('CONTRACTSUM').setReadOnly(false);
        }
        else {
            T11Form.getForm().findField('M_NHIKEY').setReadOnly(false);
            T11Form.getForm().findField('HEALTHOWNEXP').setReadOnly(false);
            T11Form.getForm().findField('DRUGSNAME').setReadOnly(false);
            T11Form.getForm().findField('MMNAME_E').setReadOnly(false);
            T11Form.getForm().findField('MMNAME_C').setReadOnly(false);
            T11Form.getForm().findField('M_PHCTNCO').setReadOnly(false);
            T11Form.getForm().findField('M_ENVDT').setReadOnly(false);
            T11Form.getForm().findField('ISSUESUPPLY').setReadOnly(false);
            T11Form.getForm().findField('E_MANUFACT').setReadOnly(false);
            T12Form.getForm().findField('BASE_UNIT').setReadOnly(false);
            T12Form.getForm().findField('M_PURUN').setReadOnly(false);
            T12Form.getForm().findField('UNITRATE').setReadOnly(false);
            T12Form.getForm().findField('MAT_CLASS_SUB').setReadOnly(false);
            T12Form.getForm().findField('COMMON').setReadOnly(false);
            T12Form.getForm().findField('TRUTRATE').setReadOnly(false);
            T12Form.getForm().findField('ONECOST').setReadOnly(false);
            T12Form.getForm().findField('HEALTHPAY').setReadOnly(false);
            T12Form.getForm().findField('COSTKIND').setReadOnly(false);
            T12Form.getForm().findField('E_RESTRICTCODE').setReadOnly(false);
            T12Form.getForm().findField('WARBAK').setReadOnly(false);
            T12Form.getForm().findField('WASTKIND').setReadOnly(false);
            T12Form.getForm().findField('SPXFEE').setReadOnly(false);
            T12Form.getForm().findField('ORDERKIND').setReadOnly(false);
            T12Form.getForm().findField('CASEDOCT').setReadOnly(false);
            T13Form.getForm().findField('CANCEL_ID').setReadOnly(false);
            T13Form.getForm().findField('M_STOREID').setReadOnly(false);
            //T13Form.getForm().findField('E_ORDERDCFLAG').setReadOnly(false);
            T13Form.getForm().findField('DRUGKIND').setReadOnly(false);
            T13Form.getForm().findField('SPDRUG').setReadOnly(false);
            T13Form.getForm().findField('FASTDRUG').setReadOnly(false);
            T13Form.getForm().findField('SPMMCODE').setReadOnly(false);
            T13Form.getForm().findField('APPQTY_TIMES').setReadOnly(false);
            T13Form.getForm().findField('ISIV').setReadOnly(false);
            T13Form.getForm().findField('E_RETURNDRUGFLAG').setReadOnly(false);
            T13Form.getForm().findField('E_VACCINE').setReadOnly(false);
            T2Form.getForm().findField('DISCOUNT_QTY').setReadOnly(false);
            T2Form.getForm().findField('DISC_COST_UPRICE').setReadOnly(false);
            T2Form.getForm().findField('M_AGENNO').setReadOnly(false);
            T2Form.getForm().findField('M_AGENLAB').setReadOnly(false);
            T2Form.getForm().findField('CASENO').setReadOnly(false);
            T2Form.getForm().findField('E_SOURCECODE').setReadOnly(false);
            T2Form.getForm().findField('M_CONTID').setReadOnly(false);
            T2Form.getForm().findField('TOUCHCASE').setReadOnly(false);
            T2Form.getForm().findField('E_CODATE_T').setReadOnly(false);
            T2Form.getForm().findField('E_ITEMARMYNO').setReadOnly(false);
            T2Form.getForm().findField('NHI_PRICE').setReadOnly(false);
            T2Form.getForm().findField('DISC_CPRICE').setReadOnly(false);
            T2Form.getForm().findField('M_CONTPRICE').setReadOnly(false);
            T2Form.getForm().findField('CONTRACTAMT').setReadOnly(false);
            T2Form.getForm().findField('CONTRACTSUM').setReadOnly(false);
            T2Form.getForm().findField('BEGINDATE_14_T').setReadOnly(false);
            T2Form.getForm().findField('EFFSTARTDATE_T').setReadOnly(false);
        }
        Ext.getCmp('btnSave').setDisabled(false);
        Ext.getCmp('btnCancel').setDisabled(false);
        T2Load();
        WEBAPP.utils.Common.cleanForms([T11Form, T12Form, T13Form, T2Form]);
    }
    function T1Submit() {
        var f = T2Form.getForm();
        if (T11Form.getForm().isValid() && T12Form.getForm().isValid() && T13Form.getForm().isValid() && T2Form.getForm().isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            T1Query.getForm().reset();
                            T1Query.getForm().findField('P2').setValue(v.MMCODE);
                            T1Load(true, true);
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Query.getForm().findField('P2').setValue(v.MMCODE);
                            T1Load(false, true);
                            msglabel('訊息區:資料修改成功');
                            break;
                    }
                    T1Cleanup();
                    T2Store.removeAll();
                    TATabs.setActiveTab('Tab1');

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

    var col_labelWid = 120;
    var col_Wid = 250;
    var fieldsetWid = 1200;

    var T11Form = Ext.widget({
        bodyStyle: 'margin:5px;border:none',
        xtype: 'form',
        autoScroll: true,
        frame: false,
        title: '',
        bodyPadding: '0 4 0 4',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: col_labelWid,
            width: col_Wid,
            msgTarget: 'side',
            labelAlign: 'right'
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                items:
                [
                    { xtype: 'hidden', name: 'x' },
                    {
                        xtype: 'textfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 13,
                        //regexText: '只能輸入英文字母與數字',
                        //regex: /^[\w-]{0,13}$/,
                        allowBlank: false,
                        fieldCls: 'required',
                        readOnly: true,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        colspan: 1,
                        listeners: {
                            blur: function () {
                                //Ext.getCmp('btnSearch').setDisabled(false);
                                if (T11Form.getForm().findField('x').getValue() == 'I') {
                                    Ext.Ajax.request({
                                        url: '/api/AA0158/CheckMmcodeExists',
                                        method: reqVal_p,
                                        params: {
                                            MMCODE: T11Form.getForm().findField('MMCODE').getValue()
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {

                                            } else {
                                                Ext.MessageBox.alert('提示', data.msg);
                                            }
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                            }
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'search',
                        id: 'btnSearch',
                        disabled: true,
                        text: '讀HIS資料', hidden: (isAB == true),
                        colspan: 1,
                        handler: function () {
                            msglabel('訊息區:');
                            if (T11Form.getForm().findField('MMCODE').getValue() == '') {
                                Ext.Msg.alert('提醒', '請輸入院內碼');
                            }
                            else {
                                Ext.Ajax.request({
                                    url: '/api/AA0158/GetHisdata',
                                    method: reqVal_p,
                                    params: {
                                        MMCODE: T11Form.getForm().findField('MMCODE').getValue()
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            var v = data.etts[0];
                                            if (v) {
                                                Ext.MessageBox.confirm('讀HIS資料', '取得HIS資料：<br>中文品名=' + v.MMNAME_C + ',<br>英文品名=' + v.MMNAME_E + ',<br>藥材單位=' + v.BASE_UNIT + ',<br>健保碼=' + v.M_NHIKEY + '<br>是否帶到藥衛材系統?', function (btn, text) {
                                                    if (btn === 'yes') {
                                                        T11Form.getForm().findField('MMNAME_C').setValue(v.MMNAME_C);
                                                        T11Form.getForm().findField('MMNAME_E').setValue(v.MMNAME_E);
                                                        T12Form.getForm().findField('BASE_UNIT').setValue(v.BASE_UNIT);
                                                        T11Form.getForm().findField('M_NHIKEY').setValue(v.M_NHIKEY);
                                                    }
                                                });
                                            }
                                            else {
                                                Ext.Msg.alert('訊息', '此院內碼查無資料');
                                            }
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '條碼代碼',
                        name: 'BARCODE',
                        labelWidth: col_labelWid,
                        readOnly: true,
                        hidden: true,
                        width: col_Wid,
                        colspan: 1
                    }, {
                        xtype: 'displayfield',
                        //hidden: true,  //此欄位為了學名可以換行,不能設hidden會失效
                        width: col_Wid * 2,
                        colspan: 2
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '學名',
                        name: 'DRUGSNAME',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 300,
                        labelWidth: col_labelWid,
                        width: col_Wid * 3,
                        readOnly: true,
                        colspan: 3
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 250,
                        labelWidth: col_labelWid,
                        width: col_Wid * 3,
                        readOnly: true,
                        colspan: 3
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 250,
                        labelWidth: col_labelWid,
                        width: col_Wid * 3,
                        readOnly: true,
                        colspan: 3
                    },

                    {
                        xtype: 'textfield',
                        fieldLabel: '健保代碼',
                        name: 'M_NHIKEY',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 12,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '健保自費碼',
                        name: 'HEALTHOWNEXP',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 12,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '許可證號',
                        name: 'M_PHCTNCO',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '許可證效期',
                        name: 'M_ENVDT',
                        id: 'M_ENVDT',
                        enforceMaxLength: true,
                        maxLength: 7,          //最多輸入7碼
                        maskRe: /[0-9]/,      //前端只能輸入數字
                        //regexText: '正確格式應為「YYYMMDD」，<br>例如「1080101」',
                        //regex: /^[0-9]{7}$/,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'textfield',
                        fieldLabel: '申請廠商',
                        name: 'ISSUESUPPLY',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '製造商',
                        name: 'E_MANUFACT',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    }
                ]
            }
        ]
    });

    var T12Form = Ext.widget({
        bodyStyle: 'margin:5px;border:none',
        xtype: 'form',
        autoScroll: true,
        frame: false,
        title: '',
        bodyPadding: '0 4 0 4',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: col_labelWid,
            width: col_Wid,
            msgTarget: 'side',
            labelAlign: 'right'
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                items:
                [
                    { xtype: 'hidden', name: 'x' },
                    {
                        xtype: 'combo',
                        store: baseunitFormStore,
                        id: 'BASE_UNIT',
                        name: 'BASE_UNIT',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        fieldLabel: '藥材單位',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        readOnly: true,
                        colspan: 1,
                        anyMatch: true,
                        listeners: {
                            select: function (c, r, i, e) {
                                var f = T12Form.getForm();
                                f.findField("M_PURUN").setValue(r.get('VALUE'));
                            }
                        }
                    }, {
                        xtype: 'combo',
                        store: baseunitFormStore,
                        id: 'M_PURUN',
                        name: 'M_PURUN',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        fieldLabel: '出貨包裝單位',
                        queryMode: 'local',
                        autoSelect: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        anyMatch: true,
                        matchFieldWidth: false,
                        listConfig: { width: 300 },
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '每包裝出貨量',
                        name: 'UNITRATE',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'combo',
                        store: matclassSubFormStore,
                        id: 'MAT_CLASS_SUB',
                        name: 'MAT_CLASS_SUB',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '物料子類別',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1,
                        anyMatch: true,
                        listeners: {
                            select: function (c, r, i, e) {
                            }
                        }
                    }, {
                        xtype: 'combo',
                        store: CommonStore,
                        id: 'COMMON',
                        name: 'COMMON',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否常用品項',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '與HIS單位換算比值',
                        name: 'TRUTRATE',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'combo',
                        store: OnecostStore,
                        id: 'ONECOST',
                        name: 'ONECOST',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否可單一計價',
                        queryMode: 'local',
                        autoSelect: true,
                        allowBlank: false,
                        //fieldCls: 'required',
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1,
                        listeners: {
                            select: function (c, r, i, e) {
                                if (r.get('VALUE') == '1') {
                                    T12Form.getForm().findField('HEALTHPAY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                    T12Form.getForm().findField('HEALTHPAY').allowBlank = false;
                                    T12Form.getForm().findField('HEALTHPAY').fieldCls = 'required';
                                    T12Form.getForm().findField('HEALTHPAY').setReadOnly(false);
                                    T12Form.getForm().findField('COSTKIND').allowBlank = true;
                                    T12Form.getForm().findField('COSTKIND').fieldCls = '';
                                    T12Form.getForm().findField('COSTKIND').setValue('');
                                    T12Form.getForm().findField('COSTKIND').setReadOnly(true);
                                }
                                if (r.get('VALUE') == '2') {
                                    T12Form.getForm().findField('COSTKIND').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                    T12Form.getForm().findField('COSTKIND').allowBlank = false;
                                    T12Form.getForm().findField('COSTKIND').fieldCls = 'required';
                                    T12Form.getForm().findField('COSTKIND').setReadOnly(false);
                                    T12Form.getForm().findField('HEALTHPAY').allowBlank = true;
                                    T12Form.getForm().findField('HEALTHPAY').fieldCls = '';
                                    T12Form.getForm().findField('HEALTHPAY').setValue('');
                                    T12Form.getForm().findField('HEALTHPAY').setReadOnly(true);
                                }
                            }
                        }

                    },
                    {
                        xtype: 'combo',
                        store: HealthPayStore,
                        id: 'HEALTHPAY',
                        name: 'HEALTHPAY',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否健保給付',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: CostKindStore,
                        id: 'COSTKIND',
                        name: 'COSTKIND',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '費用分類',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'combo',
                        store: eRestrictCodeStore,
                        id: 'E_RESTRICTCODE',
                        name: 'E_RESTRICTCODE',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '管制級數',
                        queryMode: 'local',
                        autoSelect: true,
                        allowBlank: false,
                        //fieldCls: 'required',
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: WarbakStore,
                        id: 'WARBAK',
                        name: 'WARBAK',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否戰備',
                        queryMode: 'local',
                        autoSelect: true,
                        allowBlank: false,
                        //fieldCls: 'required',
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: WastKindStore,
                        id: 'WASTKIND',
                        name: 'WASTKIND',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否正向消耗',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'combo',
                        store: SpXfeeStore,
                        id: 'SPXFEE',
                        name: 'SPXFEE',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否為特材',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: OrderKindStore,
                        id: 'ORDERKIND',
                        name: 'ORDERKIND',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '採購類別',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '小採需求醫師',
                        name: 'CASEDOCT',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    }

                ]
            }
        ]
    });


    var T13Form = Ext.widget({
        bodyStyle: 'margin:5px;border:none',
        xtype: 'form',
        autoScroll: true,
        frame: false,
        title: '',
        bodyPadding: '0 4 0 4',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: col_labelWid,
            width: col_Wid,
            msgTarget: 'side',
            labelAlign: 'right'
        },
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 3
                },
                items:
                [
                    { xtype: 'hidden', name: 'x' },
                    {
                        xtype: 'combo',
                        store: form_YN,
                        id: 'CANCEL_ID',
                        name: 'CANCEL_ID',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否作廢',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        readOnly: true,
                        colspan: 1,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                    },
                    //{
                    //    xtype: 'combo',
                    //    store: form_YN,
                    //    id: 'E_ORDERDCFLAG',
                    //    name: 'E_ORDERDCFLAG',
                    //    displayField: 'COMBITEM',
                    //    valueField: 'VALUE',
                    //    fieldLabel: '藥品是否停用',
                    //    queryMode: 'local',
                    //    autoSelect: true,
                    //    multiSelect: false,
                    //    allowBlank: false,
                    //    //fieldCls: 'required',
                    //    readOnly: true,
                    //    colspan: 1,
                    //    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                    //},
                    {
                        xtype: 'combo',
                        store: MStoreidStore,
                        id: 'M_STOREID',
                        name: 'M_STOREID',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '庫備識別碼',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'combo',
                        store: DrugKindStore,
                        id: 'DRUGKIND',
                        name: 'DRUGKIND',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '中西藥類別',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: IsivStore,
                        id: 'ISIV',
                        name: 'ISIV',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '是否點滴',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 2
                    },

                    {
                        xtype: 'combo',
                        store: SpDrugStore,
                        id: 'SPDRUG',
                        name: 'SPDRUG',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '特殊品項',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '特材號碼',
                        name: 'SPMMCODE',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 120,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: FastDrugStore,
                        id: 'FASTDRUG',
                        name: 'FASTDRUG',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '急救品項',
                        queryMode: 'local',
                        allowBlank: false,
                        fieldCls: 'required',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '申請倍數',
                        name: 'APPQTY_TIMES',
                        id: 'APPQTY_TIMES',
                        enforceMaxLength: true,
                        maskRe: /[1-9]/,      //前端只能輸入數字
                        //regex: /^[1-9]\d*$/,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1,
                        listeners: {
                            change: function (e, text, prev) {
                                if (!/^[1-9]\d*$/.test(text) && text!='') {
                                    e.setValue(prev);  // 回復原值
                                }
                            }
                        }
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '藥衛材基本檔修改歷程序號',
                        name: 'MIMASTHIS_SEQ',
                        submitValue: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: form_YN2,
                        id: 'E_RETURNDRUGFLAG',
                        name: 'E_RETURNDRUGFLAG',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '疾管署疫苗',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    }, ,
                    {
                        xtype: 'combo',
                        store: form_YN2,
                        id: 'E_VACCINE',
                        name: 'E_VACCINE',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '疫苗',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    }

                ]
            }
        ]
    });
    var T2Form = Ext.widget({
        bodyStyle: 'margin:5px;border:none',
        xtype: 'form',
        autoScroll: true,
        frame: false,
        title: '',
        bodyPadding: '0 4 0 4',
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: col_labelWid,
            width: col_Wid,
            msgTarget: 'side',
            labelAlign: 'right'
        },
        items: [
            { xtype: 'hidden', name: 'x' },
            //T11Form
            { xtype: 'hidden', name: 'MMCODE' },
            { xtype: 'hidden', name: 'M_NHIKEY' },
            { xtype: 'hidden', name: 'HEALTHOWNEXP' },
            { xtype: 'hidden', name: 'DRUGSNAME' },
            { xtype: 'hidden', name: 'MMNAME_E' },
            { xtype: 'hidden', name: 'MMNAME_C' },
            { xtype: 'hidden', name: 'M_PHCTNCO' },
            { xtype: 'hidden', name: 'M_ENVDT' },
            { xtype: 'hidden', name: 'ISSUESUPPLY' },
            { xtype: 'hidden', name: 'E_MANUFACT' },
            //T12Form
            { xtype: 'hidden', name: 'BASE_UNIT' },
            { xtype: 'hidden', name: 'M_PURUN' },
            { xtype: 'hidden', name: 'UNITRATE' },
            { xtype: 'hidden', name: 'MAT_CLASS_SUB' },
            { xtype: 'hidden', name: 'COMMON' },
            { xtype: 'hidden', name: 'TRUTRATE' },
            { xtype: 'hidden', name: 'ONECOST' },
            { xtype: 'hidden', name: 'HEALTHPAY' },
            { xtype: 'hidden', name: 'COSTKIND' },
            { xtype: 'hidden', name: 'E_RESTRICTCODE' },
            { xtype: 'hidden', name: 'WARBAK' },
            { xtype: 'hidden', name: 'WASTKIND' },
            { xtype: 'hidden', name: 'SPXFEE' },
            { xtype: 'hidden', name: 'ORDERKIND' },
            { xtype: 'hidden', name: 'CASEDOCT' },
            //T13Form
            { xtype: 'hidden', name: 'CANCEL_ID' },
            { xtype: 'hidden', name: 'M_STOREID' },
            //{ xtype: 'hidden', name: 'E_ORDERDCFLAG' },
            { xtype: 'hidden', name: 'DRUGKIND' },
            { xtype: 'hidden', name: 'SPDRUG' },
            { xtype: 'hidden', name: 'SPMMCODE' },
            { xtype: 'hidden', name: 'FASTDRUG' },
            { xtype: 'hidden', name: 'MIMASTHIS_SEQ' },
            { xtype: 'hidden', name: 'APPQTY_TIMES' },
            { xtype: 'hidden', name: 'ISIV' },
            { xtype: 'hidden', name: 'E_RETURNDRUGFLAG' },
            { xtype: 'hidden', name: 'E_VACCINE' },
            { xtype: 'hidden', name: 'MONEYCHANGE' },
            { xtype: 'hidden', name: 'JBID_RCRATE' },
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 4
                },
                items:
                [
                    T2FormAgenno,
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠商名稱',
                        name: 'AGEN_NAMEC',
                        labelAlign: 'right',
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    Ext.create('WEBAPP.form.QueryCombo', {
                        fieldLabel: '合約案號',
                        queryUrl: '/api/AA0158/GetCurrCaseNoCombo',
                        extraFields: ['EXTRA1'],
                        id: "t2formcaseno",
                        name: 'CASENO',
                        enforceMaxLength: true,
                        maxLength: 30,
                        labelWidth: col_labelWid,
                        width: col_Wid * 2,
                        displayField: 'VALUE',
                        readOnly: true,
                        colspan: 2,
                        listeners: {
                            select: function (c, r, i, e) {
                                if (r) {
                                    T2Form.getForm().findField('JBID_RCRATE').setValue(r.data.EXTRA1);
                                } else {
                                    T2Form.getForm().findField('JBID_RCRATE').setValue(null);
                                }
                            },
                            blur: function (field, eOpts) {
                                var v = this.getValue();
                                var r = this.findRecord(this.valueField || this.displayField, v);
                                if (this.store.indexOf(r) === -1 && this.readOnly === false) {
                                    this.setValue(null);
                                    Ext.MessageBox.alert('訊息', '若合約案號CaseNo未建檔，請於 合約優惠設定作業 建立資料。');
                                    return;
                                }

                                calDiscPrice();
                            }
                        }
                    }),
                    {
                        xtype: 'textfield',
                        fieldLabel: '廠牌',
                        name: 'M_AGENLAB',
                        enforceMaxLength: true,
                        maxLength: 64,
                        labelWidth: col_labelWid,
                        width: col_Wid * 2,
                        readOnly: true,
                        colspan: 2
                    },
                    //
                    {
                        xtype: 'combo',
                        store: TouchCaseStore,
                        id: 'TOUCHCASE',
                        name: 'TOUCHCASE',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '合約類別',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        allowBlank: false,
                        //fieldCls: 'required',
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '合約到期日',
                        name: 'E_CODATE_T',
                        labelAlign: 'right',
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: eSourceCodeStore,
                        id: 'E_SOURCECODE',
                        name: 'E_SOURCECODE',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '付款方式',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        allowBlank: false,
                        //fieldCls: 'required',
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'combo',
                        store: MContidStore,
                        id: 'M_CONTID',
                        name: 'M_CONTID',
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        fieldLabel: '合約方式',
                        queryMode: 'local',
                        autoSelect: true,
                        multiSelect: false,
                        allowBlank: false,
                        //fieldCls: 'required',
                        typeAhead: true, forceSelection: true, selectOnFocus: true,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '成本價',
                        name: 'DISC_CPRICE',
                        labelAlign: 'right',
                        validator: function (v) {
                            return /^-?[0-9]*(\.[0-9]{1,4})?$/.test(v) ? true : '只能輸入小數點四位!';
                        },
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        minValue: 0,
                        colspan: 1,
                        listeners: {
                            change: function () {
                                calDiscPrice();
                            }
                        }
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '決標價',
                        name: 'M_CONTPRICE',
                        labelAlign: 'right',
                        validator: function (v) {
                            return /^-?[0-9]*(\.[0-9]{1,4})?$/.test(v) ? true : '只能輸入小數點四位!';
                        },
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        minValue: 0,
                        colspan: 1,
                        listeners: {
                            change: function () {
                                calDiscPrice();
                            }
                        }
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '二次折讓數量',
                        name: 'DISCOUNT_QTY',
                        id: 'DISCOUNT_QTY',
                        enforceMaxLength: true,
                        labelWidth: col_labelWid,
                        minValue: 0,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'numberfield',
                        fieldLabel: '二次優惠單價',
                        name: 'DISC_COST_UPRICE',
                        id: 'DISC_COST_UPRICE',
                        enforceMaxLength: true,
                        minValue: 0,
                        validator: function (v) {
                            return /^-?[0-9]*(\.[0-9]{1,2})?$/.test(v) ? true : '只能輸入小數點二位!';
                        },
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '',
                        colspan: 1
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '折讓比',
                        name: 'DISC_RATIO',
                        colspan: 1
                    },
                    {
                        xtype: 'textfield',
                        fieldLabel: '聯標項次',
                        name: 'E_ITEMARMYNO',
                        labelAlign: 'right',
                        enforceMaxLength: true,
                        maxLength: 50,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '健保價',
                        name: 'NHI_PRICE',
                        labelAlign: 'right',
                        maskRe: /[0-9.]/,
                        validator: function (v) {
                            return /^-?[0-9]*(\.[0-9]{1,4})?$/.test(v) ? true : '只能輸入小數點四位!';
                        },
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        readOnly: true,
                        colspan: 1
                    },

                    {
                        xtype: 'numberfield',
                        fieldLabel: '聯標契約總數量',
                        name: 'CONTRACTAMT',
                        labelAlign: 'right',
                        maskRe: /[0-9.]/,
                        regexText: '只能輸入數字及小數點',
                        regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                        labelWidth: col_labelWid,
                        width: col_Wid,
                        //allowBlank: false,
                        //fieldCls: 'required',
                        minValue: 0,
                        readOnly: true,
                        colspan: 1
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '聯標項次契約總價',
                        name: 'CONTRACTSUM',
                        labelAlign: 'right',
                        maskRe: /[0-9.]/,
                        regexText: '只能輸入數字及小數點',
                        regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                        labelWidth: col_labelWid,
                        //allowBlank: false,
                        //fieldCls: 'required',
                        width: col_Wid,
                        minValue: 0,
                        readOnly: true,
                        colspan: 1
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '建立日期',
                        name: 'BEGINDATE_14_T',
                        readOnly: true,
                        submitValue: false,
                        colspan: 1
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '生效日期',
                        name: 'EFFSTARTDATE_T',
                        readOnly: true,
                        submitValue: false,
                        colspan: 1
                    }

                ]
            },
            {
                xtype: 'button',
                text: '儲存',
                id: 'btnSave',
                disabled: true, hidden: (isAB == true),
                handler: function () {
                    if (T11Form.getForm().isValid() && T12Form.getForm().isValid() && T13Form.getForm().isValid() && T2Form.getForm().isValid()) {
                        if (
                            !T2Form.isDirty() &&
                            !T11Form.isDirty() &&
                            !T12Form.isDirty() &&
                            !T13Form.isDirty()
                        ) {
                            T1Cleanup();
                        }
                        else {
                            setT2Form();
                            var confirmSubmit = '儲存';
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定儲存?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                } else {
                                    T2Form.getForm().findField('MONEYCHANGE').reset();
                                }
                            }
                            );
                        }
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                xtype: 'button',
                id: 'btnCancel',
                disabled: true,
                text: '取消', hidden: (isAB == true),
                handler: function () {
                    T1Cleanup();
                }
            }
        ]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'setprice', text: '重設單價', disabled: true, hidden: (isAB == true),
                handler: function () {
                    Ext.Ajax.request({
                        url: '/api/AA0158/SetPrice',
                        method: reqVal_p,
                        params: {
                            MIMASTHIS_SEQ: T2LastRec.data['MIMASTHIS_SEQ']
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                Ext.MessageBox.alert('訊息', '重設單價成功');
                                T1Cleanup();
                                T2Store.removeAll();
                                // Bug 1. 先塞 T1LastRec，正確做法為重新刷新 T1LastRec 或是 使用別的方式
                                setFormT1a();
                                TATabs.setActiveTab('Tab1');
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });


                }
            }
        ]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MIMASTHIS_SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'EFFSTARTDATE', type: 'string' },
            { name: 'EFFSTARTDATE_T', type: 'string' },
            { name: 'EFFENDDATE', type: 'string' },
            { name: 'EFFENDDATE_T', type: 'string' },
            { name: 'CANCEL_ID', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            //{ name: 'E_ORDERDCFLAG', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'HEALTHOWNEXP', type: 'string' },
            { name: 'DRUGSNAME', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'M_PHCTNCO', type: 'string' },
            { name: 'M_ENVDT', type: 'string' },
            { name: 'ISSUESUPPLY', type: 'string' },
            { name: 'E_MANUFACT', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'UNITRATE', type: 'string' },
            { name: 'MAT_CLASS_SUB', type: 'string' },
            { name: 'E_RESTRICTCODE', type: 'string' },
            { name: 'WARBAK', type: 'string' },
            { name: 'WARBAK_DESC', type: 'string' },
            { name: 'ONECOST', type: 'string' },
            { name: 'ONECOST_DESC', type: 'string' },
            { name: 'HEALTHPAY', type: 'string' },
            { name: 'HEALTHPAY_DESC', type: 'string' },
            { name: 'COSTKIND', type: 'string' },
            { name: 'COSTKIND_DESC', type: 'string' },
            { name: 'WASTKIND', type: 'string' },
            { name: 'WASTKIND_DESC', type: 'string' },
            { name: 'SPXFEE', type: 'string' },
            { name: 'SPXFEE_DESC', type: 'string' },
            { name: 'ORDERKIND', type: 'string' },
            { name: 'ORDERKIND_DESC', type: 'string' },
            { name: 'DRUGKIND', type: 'string' },
            { name: 'DRUGKIND_DESC', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'EASYNAME', type: 'string' },
            { name: 'M_AGENLAB', type: 'string' },
            { name: 'CASENO', type: 'string' },
            { name: 'E_SOURCECODE', type: 'string' },
            { name: 'E_SOURCECODE_DESC', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_DESC', type: 'string' },
            { name: 'E_ITEMARMYNO', type: 'string' },
            { name: 'NHI_PRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'E_CODATE', type: 'string' },
            { name: 'E_CODATE_T', type: 'string' },
            { name: 'CONTRACTAMT', type: 'string' },
            { name: 'CONTRACTSUM', type: 'string' },
            { name: 'TOUCHCASE', type: 'string' },
            { name: 'TOUCHCASE_DESC', type: 'string' },
            { name: 'BEGINDATE_14', type: 'string' },
            { name: 'BEGINDATE_14_T', type: 'string' },
            { name: 'ISSPRICEDATE', type: 'string' },
            { name: 'ISSPRICEDATE_T', type: 'string' },
            { name: 'SPDRUG', type: 'string' },
            { name: 'FASTDRUG', type: 'string' },
            { name: 'DISCOUNT_QTY', type: 'string' },
            { name: 'APPQTY_TIMES', type: 'string' },
            { name: 'ISIV', type: 'string' },
            { name: 'E_RETURNDRUGFLAG', type: 'string' },
            { name: 'E_VACCINE', type: 'string' },
            { name: 'DISC_COST_UPRICE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_TIME_T', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MIMASTHIS_SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0158/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }, listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np;
                if (T1LastRec) {
                    np = {
                        p0: T1LastRec.data['MMCODE']
                    };
                } else {
                    np = {
                        p0: ''
                    };
                }
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
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
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        autoScroll: true,
        //loadingText: '處理中...',
        //loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "序號", dataIndex: 'MIMASTHIS_SEQ', width: 100 },
            //{ text: "院內碼", dataIndex: 'MMCODE', width: 120 },
            { text: "廠商代碼", dataIndex: 'M_AGENNO', width: 100 },
            { text: "廠商簡稱", dataIndex: 'EASYNAME', width: 100 },
            { text: "廠牌", dataIndex: 'M_AGENLAB', width: 100 },
            { text: "合約案號", dataIndex: 'CASENO', width: 100 },
            { text: "聯標項次", dataIndex: 'E_ITEMARMYNO', width: 100 },
            { text: "健保價", dataIndex: 'NHI_PRICE', width: 80, style: 'text-align:left', align: 'right' },
            { text: "決標價", dataIndex: 'M_CONTPRICE', width: 80, style: 'text-align:left', align: 'right' },
            { text: "成本價", dataIndex: 'DISC_CPRICE', width: 100, style: 'text-align:left', align: 'right' },
            { text: "付款方式", dataIndex: 'E_SOURCECODE_DESC', width: 100 },
            { text: "合約方式", dataIndex: 'M_CONTID_DESC', width: 100 },
            { text: "合約到期日", dataIndex: 'E_CODATE_T', width: 100 },
            { text: "合約類別", dataIndex: 'TOUCHCASE_DESC', width: 100 },
            { text: "建立日期", dataIndex: 'BEGINDATE_14_T', width: 100 },
            { text: "單價生效日", dataIndex: 'ISSPRICEDATE_T', width: 100 },
            { text: "聯標契約數量", dataIndex: 'CONTRACTAMT', width: 100, style: 'text-align:left', align: 'right' },
            { text: "聯標契約總價", dataIndex: 'CONTRACTSUM', width: 100, style: 'text-align:left', align: 'right' },
            { text: "生效起始時間", dataIndex: 'EFFSTARTDATE_T', width: 120 },
            { text: "生效結束時間", dataIndex: 'EFFENDDATE_T', width: 120 },
            { text: "建立時間", dataIndex: 'CREATE_TIME', width: 100 },
            { text: "建立人員", dataIndex: 'CREATE_USER', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                T2Grid.down('#setprice').setDisabled(T2Rec === 0);
            }
        }
    });

    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'textfield',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                labelAlign: 'right',
                enforceMaxLength: true,
                maxLength: 13,
                //regexText: '只能輸入英文字母與數字',
                //regex: /^[\w-]{0,13}$/,
                //allowBlank: false,
                fieldCls: 'required',
                labelWidth: col_labelWid,
                width: col_Wid
            }
        ],
        buttons: [{
            itemId: 'apply', id: 'T3btn1', text: '確定',
            //disabled: true,
            handler: function () {
                if (T3Form.getForm().findField('MMCODE').getValue() == '') {
                    Ext.Msg.alert('提醒', '請輸入院內碼');
                }
                else {
                    submitApply();
                }
            }
        },
        {
            itemId: 'cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [
            {
                itemId: 'Tab1',
                title: '基本資料',
                items: [T11Form]
            }, {
                itemId: 'Tab2',
                title: '分類資料1',
                items: [T12Form]
            }, {
                itemId: 'Tab3',
                title: '分類資料2',
                items: [T13Form]
            }
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
            items: [{
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
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '34%',
                        items: [TATabs]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '33%',
                        split: true,
                        items: [T2Form]
                    },
                    {
                        region: 'south',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '33%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'west',
            collapsible: true,
            floatable: true,
            width: 240,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Grid]
        }
        ]
    });

    var win3;
    var winActWidth2 = 300;
    var winActHeight2 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '複製轉新增',
            closeAction: 'hide',
            width: winActWidth2,
            height: winActHeight2,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth2 > 0) ? winActWidth2 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight2 > 0) ? winActHeight2 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth2 = width;
                    winActHeight2 = height;
                }
            }
        });
    }
    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }

    function submitApply() {
        Ext.MessageBox.confirm('複製轉新增', '是否確定複製轉新增？', function (btn, text) {
            if (btn === 'yes') {
                Ext.Ajax.request({
                    url: '/api/AA0158/Copy',
                    method: reqVal_p,
                    params: {
                        OLDMMCODE: T1LastRec.data['MMCODE'],
                        NEWMMCODE: T3Form.getForm().findField("MMCODE").getValue()
                    },
                    //async: true,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            msglabel('訊息區:複製轉新增成功');
                            var v = data.etts[0];
                            T1Cleanup();
                            T2Store.removeAll();
                            T1Query.getForm().reset();
                            T1Query.getForm().findField('P2').setValue(v.MMCODE);
                            T1Load(true, true);
                        }
                        else
                            Ext.MessageBox.alert('錯誤', data.msg);
                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    }
                });
            }
        }
        );
        hideWin3();
    }

    var winActWidth = viewport.width - 20;
    var winActHeight = viewport.height - 20;
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MIMASTHIS_SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'EFFSTARTDATE', type: 'string' },
            { name: 'EFFENDDATE', type: 'string' },
            { name: 'CANCEL_ID', type: 'string' },
            { name: 'E_ORDERDCFLAG', type: 'string' },
            { name: 'M_STOREID', type: 'string' },
            { name: 'M_STOREID_DESC', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'HEALTHOWNEXP', type: 'string' },
            { name: 'DRUGSNAME', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'M_PHCTNCO', type: 'string' },
            { name: 'M_ENVDT', type: 'string' },
            { name: 'ISSUESUPPLY', type: 'string' },
            { name: 'E_MANUFACT', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'UNITRATE', type: 'string' },
            { name: 'MAT_CLASS_SUB', type: 'string' },
            { name: 'COMMON', type: 'string' },
            { name: 'TRUTRATE', type: 'string' },
            { name: 'E_RESTRICTCODE', type: 'string' },
            { name: 'WARBAK', type: 'string' },
            { name: 'WARBAK_DESC', type: 'string' },
            { name: 'ONECOST', type: 'string' },
            { name: 'ONECOST_DESC', type: 'string' },
            { name: 'HEALTHPAY', type: 'string' },
            { name: 'HEALTHPAY_DESC', type: 'string' },
            { name: 'COSTKIND', type: 'string' },
            { name: 'COSTKIND_DESC', type: 'string' },
            { name: 'WASTKIND', type: 'string' },
            { name: 'WASTKIND_DESC', type: 'string' },
            { name: 'SPXFEE', type: 'string' },
            { name: 'SPXFEE_DESC', type: 'string' },
            { name: 'ORDERKIND', type: 'string' },
            { name: 'ORDERKIND_DESC', type: 'string' },
            { name: 'CASEDOCT', type: 'string' },
            { name: 'DRUGKIND', type: 'string' },
            { name: 'DRUGKIND_DESC', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'EASYNAME', type: 'string' },
            { name: 'M_AGENLAB', type: 'string' },
            { name: 'CASENO', type: 'string' },
            { name: 'E_SOURCECODE', type: 'string' },
            { name: 'E_SOURCECODE_DESC', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'M_CONTID_DESC', type: 'string' },
            { name: 'E_ITEMARMYNO', type: 'string' },
            { name: 'NHI_PRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'E_CODATE', type: 'string' },
            { name: 'E_CODATE_T', type: 'string' },
            { name: 'CONTRACTAMT', type: 'string' },
            { name: 'CONTRACTSUM', type: 'string' },
            { name: 'TOUCHCASE', type: 'string' },
            { name: 'TOUCHCASE_DESC', type: 'string' },
            { name: 'BEGINDATE_14', type: 'string' },
            { name: 'BEGINDATE_14_T', type: 'string' },
            { name: 'ISSPRICEDATE_T', type: 'string' },
            { name: 'SPDRUG', type: 'string' },
            { name: 'SPMMCODE', type: 'string' },
            { name: 'FASTDRUG', type: 'string' },
            { name: 'DISCOUNT_QTY', type: 'string' },
            { name: 'APPQTY_TIMES', type: 'string' },
            { name: 'ISIV', type: 'string' },
            { name: 'DISC_COST_UPRICE', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }
        ]
    });
    var T6Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 80
        },
        border: false,
        items: [
            {
                xtype: 'button',
                id: 'T6sample',
                name: 'T6sample',
                text: '範本', handler: function () {
                    var p = new Array();
                    PostForm('../../api/AA0158/GetExcelExample', p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'button',
                id: 'desciption',
                name: 'desciption',
                text: '填寫說明',
                handler: function () {
                    var p = new Array();
                    PostForm('../../api/AA0158/GetTxtExample', p);
                    msglabel('訊息區:匯出完成');
                }
            },
            {
                xtype: 'filefield',
                name: 'T6send',
                id: 'T6send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('T6insert').setDisabled(true);
                        T6Store.removeAll();
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('T6send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0158/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T6Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('T6insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T6Store.loadData(data.etts, false);
                                        IsSend = true;
                                        T6Grid.columns[1].setVisible(true);
                                        //T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('T6insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('T6send').fileInputEl.dom.value = '';
                                    Ext.getCmp('T6insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                xtype: 'button',
                text: '更新',
                id: 'T6insert',
                name: 'T6insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0158/InsertFromXls',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T6Store.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">院內碼</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "匯入<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:匯入<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('T6insert').setDisabled(true);
                                T6Store.removeAll();
                                T6Grid.columns[1].setVisible(false);
                                T1Load(true, true);
                            }
                            myMask.hide();
                            Ext.getCmp('T6insert').setDisabled(true);
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            Ext.getCmp('T6insert').setDisabled(true);
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', "匯入失敗");
                                    break;
                            }
                        }
                    });

                    hideWin6();
                }
            }
        ]
    });
    var T6Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 1000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            //url: '/api/AA0159/???',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    //p2: T1F2,
                    //p3: T1F5
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    })
    var T6Grid = Ext.create('Ext.grid.Panel', {
        autoScroll: true,
        store: T6Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Query]
            }
        ],
        columns: [
            { xtype: 'rownumberer' },
            {
                text: "檢核結果",
                dataIndex: 'CHECK_RESULT',
                hidden: true,
                width: 250
            },
            { text: "院內碼", dataIndex: 'MMCODE', width: 120 },
            { text: "學名", dataIndex: "DRUGSNAME", width: 100 },
            { text: "英文品名", dataIndex: "MMNAME_E", width: 100 },
            { text: "中文品名", dataIndex: "MMNAME_C", width: 100 },
            { text: "健保代碼", dataIndex: "M_NHIKEY", width: 100 },
            { text: "健保自費碼", dataIndex: "HEALTHOWNEXP", width: 100 },
            { text: "許可證號", dataIndex: "M_PHCTNCO", width: 100 },
            { text: "許可證效期", dataIndex: "M_ENVDT", width: 100 },
            { text: "申請廠商", dataIndex: "ISSUESUPPLY", width: 100 },
            { text: "製造商", dataIndex: "E_MANUFACT", width: 100 },
            { text: "藥材單位", dataIndex: "BASE_UNIT", width: 100 },
            { text: "出貨包裝單位", dataIndex: "M_PURUN", width: 100 },
            { text: "每包裝出貨量", dataIndex: "UNITRATE", width: 100 },
            { text: "物料子類別", dataIndex: "MAT_CLASS_SUB", width: 100 },
            { text: "是否常用品項", dataIndex: "COMMON", width: 100 },
            //{ text: "生效起始時間", dataIndex: 'EFFSTARTDATE', width: 120 },
            { text: "與HIS單位換算比值", dataIndex: "TRUTRATE", width: 100 },
            { text: "是否可單一計價", dataIndex: "ONECOST", width: 100 },
            { text: "是否健保給付", dataIndex: "HEALTHPAY", width: 100 },
            { text: "費用分類", dataIndex: "COSTKIND", width: 100 },
            { text: "管制級數", dataIndex: "E_RESTRICTCODE", width: 100 },
            { text: "是否戰備", dataIndex: "WARBAK", width: 100 },
            { text: "是否正向消耗", dataIndex: "WASTKIND", width: 100 },
            { text: "是否為特材", dataIndex: "SPXFEE", width: 100 },
            { text: "採購類別", dataIndex: "ORDERKIND", width: 100 },
            { text: "小採需求醫師", dataIndex: "CASEDOCT", width: 100 },
            { text: "是否作廢", dataIndex: "CANCEL_ID", width: 100 },
            { text: "中西藥類別", dataIndex: "DRUGKIND", width: 100 },
            { text: "是否點滴", dataIndex: "ISIV", width: 100 },
            { text: "特殊品項", dataIndex: "SPDRUG", width: 100 },
            { text: "特材號碼", dataIndex: "SPMMCODE", width: 100 },
            { text: "急救品項", dataIndex: "FASTDRUG", width: 100 },
            { text: "申請倍數", dataIndex: "APPQTY_TIMES", width: 100 },
            { text: "二次折讓數量", dataIndex: "DISCOUNT_QTY", width: 100 },
            { text: "二次優惠單價", dataIndex: "DISC_COST_UPRICE", width: 100 },
            { text: "庫備識別碼", dataIndex: "M_STOREID", width: 100 },
            { text: "廠商代碼", dataIndex: "M_AGENNO", width: 100 },
            { text: "合約案號", dataIndex: "CASENO", width: 100 },
            { text: "廠牌", dataIndex: "M_AGENLAB", width: 100 },
            { text: "合約類別", dataIndex: "TOUCHCASE", width: 100 },
            { text: "合約到期日", dataIndex: "E_CODATE", width: 100 },
            { text: "付款方式", dataIndex: "E_SOURCECODE", width: 100 },
            { text: "合約方式", dataIndex: "M_CONTID", width: 100 },
            { text: "成本價", dataIndex: "DISC_CPRICE", width: 100 },
            { text: "決標價", dataIndex: "M_CONTPRICE", width: 100 },
            { text: "聯標項次", dataIndex: "E_ITEMARMYNO", width: 100 },
            { text: "健保價", dataIndex: "NHI_PRICE", width: 100 },
            { text: "聯標契約總數量", dataIndex: "CONTRACTAMT", width: 100 },
            { text: "聯標項次契約總價", dataIndex: "CONTRACTSUM", width: 100 },
            { header: "", flex: 1 }
        ]
    });
    var win6;
    if (!win6) {
        win6 = Ext.widget('window', {
            title: '匯入',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: [T6Grid],
            buttons: [{
                text: '關閉',
                handler: function () {
                    hideWin6();
                }
            }],
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }

    function showWin6() {
        if (win6.hidden) {
            T6Cleanup();
            T6Store.removeAll();
            win6.show();
        }
    }
    function hideWin6() {
        if (!win6.hidden) {
            win6.hide();
            T6Cleanup();
        }
    }

    function T6Cleanup() {
        T6Query.getForm().reset();
        msglabel('訊息區:');
    }

    function MenuLinkSet() {
        if (menuLink == "AA0196") {
            T1Query.getForm().findField('P1').setValue("02");
        }
        else if (menuLink == "AA0198") {
            T1Query.getForm().findField('P1').setValue("01");
        }
        else if (menuLink == "AA0199" || menuLink == "AA0204") {
            Ext.getCmp('btnAdd').setDisabled(true);
            Ext.getCmp('btnExport').setDisabled(true);
            Ext.getCmp('btnIxport').setDisabled(true);
            Ext.getCmp('btnCopy').setDisabled(true);
        }
    }

    T1Query.getForm().findField('P0').focus();
    MenuLinkSet();
});
