Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1F1 = '';
    var T1F2 = '';
    var T1F3 = '';
    var T1Name = "藥品基本檔維護作業";
    var MmcodeComboGet = '/api/AA0159/GetMMCodeFromMastHisCombo';
    var MatclassComboGet = '/api/AA0159/GetMatclassCombo';
    var MatclassSubComboGet = '/api/AA0159/GetMatclassSubCombo';
    var BaseunitComboGet = '/api/AA0159/GetBaseunitCombo';
    var YonNComboGet = '/api/AA0159/GetYnCombo';
    var AgennoComboGet = '/api/AA0159/GetAgennoCombo';
    var ESourceCodeComboGet = '/api/AA0159/GetESourceCodeCombo';
    var ERestrictCodeComboGet = '/api/AA0159/GetERestrictCodeCombo';
    var WarbakComboGet = '/api/AA0159/GetWarbakCombo';
    var OnecostComboGet = '/api/AA0159/GetOnecostCombo';
    var HealthPayComboGet = '/api/AA0159/GetHealthPayCombo';
    var CostKindComboGet = '/api/AA0159/GetCostKindCombo';
    var WastKindComboGet = '/api/AA0159/GetWastKindCombo';
    var SpXfeeComboGet = '/api/AA0159/GetSpXfeeCombo';
    var OrderKindComboGet = '/api/AA0159/GetOrderKindCombo';
    var DrugKindComboGet = '/api/AA0159/GetDrugKindCombo';
    var SpDrugComboGet = '/api/AA0159/GetSpDrugCombo';
    var FastDrugComboGet = '/api/AA0159/GetFastDrugCombo';
    var MContidComboGet = '/api/AA0159/GetMContidCombo';
    var TouchCaseComboGet = '/api/AA0159/GetTouchCaseCombo';
    var CommonComboGet = '/api/AA0159/GetCommonCombo';
    var MStoreidComboGet = '/api/AA0158/MStoreidComboGet';
    var T6GetExcelExample = '/api/AA0159/GetExcelExample';
    var mode = '';

    var form_YN = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "", "TEXT": "", "COMBITEM": "" },
            { "VALUE": "Y", "TEXT": "Y", "COMBITEM": "Y 作廢" },
            { "VALUE": "N", "TEXT": "N", "COMBITEM": "N 使用" }
        ]
    });

    var T1Rec = 0;
    var T1LastRec = null;
    var isAdmin = 'N';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var menuLink = Ext.getUrlParam('menuLink');
    var matclass = "";
    if (menuLink == "AA0200") {
        matclass = "02";
    }
    else if (menuLink == "AA0021") {
        matclass = "01";
    }
    else {
        matclass = "";
    }
    var matclassFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var matclassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    var matclassSubStore = Ext.create('Ext.data.Store', {
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

    var baseunitFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
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
                            matclassSubStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, COMBITEM: tb_data[i].COMBITEM });
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
                            baseunitFormStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
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
        id: 'P0',
        name: 'P0',
        fieldLabel: '院內碼',
        labelWidth: 60,
        width: 240,
        //allowBlank: false,
        fieldCls: 'required',
        limit: 10,
        queryUrl: MmcodeComboGet,
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        getDefaultParams: function () {
            return {
                p1: matclass
            };
        },
        listeners: {
            select: function () {
                Ext.getCmp('T1btn1').setDisabled(false);
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 240;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    T1QuerymmCodeCombo,
                    {
                        xtype: 'button',
                        text: '查詢',
                        id: 'T1btn1',
                        disabled: true,
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (f.findField('P0').getValue() == null || f.findField('P0').getValue() == '') {
                                Ext.Msg.alert('提醒', '請挑選院內碼');
                            }
                            else {
                                T1Load();
                            }
                            msglabel('訊息區:');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            Ext.getCmp('T1btn1').setDisabled(true);
                        }
                    }
                ]
            }]
        } ]
    });

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
var T1Store = Ext.create('Ext.data.Store', {
    model: 'T1Model',
    pageSize: 30, // 每頁顯示筆數
    remoteSort: true,
    sorters: [{ property: 'MIMASTHIS_SEQ', direction: 'DESC' }],
    proxy: {
        type: 'ajax',
        actionMethods: {
            read: 'POST' // by default GET
        },
        url: '/api/AA0159/GetAll',
        reader: {
            type: 'json',
            rootProperty: 'etts',
            totalProperty: 'rc'
        }
    }, listeners: {
        beforeload: function (store, options) {
            store.removeAll();
            var np = {
                p0: T1Query.getForm().findField('P0').getValue()
            };
            Ext.apply(store.proxy.extraParams, np);
        },
        load: function (store, records) {
            for (var i = 0; i < records.length; i++) {
                if (records[i].data.E_CODATE != null) {
                    var e_codate = records[i].data['E_CODATE'].toString();
                    if (e_codate.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || e_codate.substring(0, 24) == 'Mon Jan 01 0001 00:00:00')
                        records[i].data['E_CODATE'] = ''; // 若資料庫值為null,在這邊另外做清除
                }
            }
            store.setData(records);
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
            text: '新增',
            itemId: 'btnAdd',
            handler: function () {
                msglabel('訊息區:');
                T1Set = '/api/AA0159/Create';
                setFormT1('I', '新增');
            }
        },
        {
            itemId: 'edit',
            text: '修改',
            disabled: true,
            handler: function () {
                msglabel('訊息區:');
                T1Set = '/api/AA0159/Update';
                setFormT1("U", '修改');
            }
        },
        {
            itemId: 'delete',
            text: '刪除',
            disabled: true,
            handler: function () {
                msglabel('訊息區:');
                Ext.MessageBox.confirm('刪除', '是否確定刪除?', function (btn, text) {
                    if (btn === 'yes') {
                        Ext.Ajax.request({
                            url: '/api/AA0159/Delete',
                            method: reqVal_p,
                            params: {
                                SEQ: T1F1,
                                SDATE: T1LastRec.data['EFFSTARTDATE_T'],
                                MMCODE: T1F3
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    Ext.MessageBox.alert('訊息', '刪除成功');
                                    T1Load();
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
            }
        },
        {
            itemId: 'import',
            text: '匯入',
            handler: function () {
                msglabel('訊息區:');
                showWin6();
            }
        },
    ]
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
        { text: "序號", dataIndex: 'MIMASTHIS_SEQ', width: 100 },
        { text: "生效起始時間", dataIndex: 'EFFSTARTDATE_T', width: 100 },
        { text: "生效結束時間", dataIndex: 'EFFENDDATE_T', width: 120 },
        { text: "院內碼", dataIndex: 'MMCODE', width: 100 },
        { text: "學名", dataIndex: 'DRUGSNAME', width: 100 },
        { text: "英文品名", dataIndex: 'MMNAME_E', width: 100 },
        { text: "中文品名", dataIndex: 'MMNAME_C', width: 100 },
        { text: "健保代碼", dataIndex: 'M_NHIKEY', width: 100 },
        { text: "健保自費碼", dataIndex: 'HEALTHOWNEXP', width: 100 },
        { text: "許可證號", dataIndex: 'M_PHCTNCO', width: 100 },
        { text: "許可證效期", dataIndex: 'M_ENVDT', width: 100 },
        { text: "申請廠商", dataIndex: 'ISSUESUPPLY', width: 100 },
        { text: "製造商", dataIndex: 'E_MANUFACT', width: 100 },
        { text: "藥材單位", dataIndex: 'BASE_UNIT', width: 100 },
        { text: "出貨包裝單位", dataIndex: 'M_PURUN', width: 100 },
        { text: "每包裝出貨量", dataIndex: 'UNITRATE', width: 100 },
        { text: "物料子類別", dataIndex: 'MAT_CLASS_SUB', width: 100 },
        { text: "是否常用品項", dataIndex: 'COMMON', width: 100 },
        { text: "與HIS單位換算比值", dataIndex: 'TRUTRATE', width: 100 },
        { text: "是否可單一計價", dataIndex: 'ONECOST', width: 100 },
        { text: "是否健保給付", dataIndex: 'HEALTHPAY', width: 100 },
        { text: "費用分類", dataIndex: 'COSTKIND', width: 100 },
        { text: "管制級數", dataIndex: 'E_RESTRICTCODE', width: 100 },
        { text: "是否戰備", dataIndex: 'WARBAK', width: 100 },
        { text: "是否正向消耗", dataIndex: 'WASTKIND', width: 100 },
        { text: "是否為特材", dataIndex: 'SPXFEE', width: 100 },
        { text: "採購類別", dataIndex: 'ORDERKIND', width: 100 },
        { text: "小採需求醫師", dataIndex: 'CASEDOCT', width: 100 },
        { text: "是否作廢", dataIndex: 'CANCEL_ID', width: 100 },
        { text: "庫備識別碼", dataIndex: 'M_STOREID', width: 100 },
        { text: "中西藥類別", dataIndex: 'DRUGKIND', width: 100 },
        { text: "是否點滴", dataIndex: 'ISIV', width: 100 },
        { text: "特殊品項", dataIndex: 'SPDRUG', width: 100 },
        { text: "特材號碼", dataIndex: 'SPMMCODE', width: 100 },
        { text: "急救品項", dataIndex: 'FASTDRUG', width: 100 },
        { text: "申請倍數", dataIndex: 'APPQTY_TIMES', width: 100 },
        { text: "廠商代碼", dataIndex: 'M_AGENNO', width: 100 },
        { text: "合約案號", dataIndex: 'CASENO', width: 100 },
        { text: "廠牌", dataIndex: 'M_AGENLAB', width: 100 },
        { text: "合約類別", dataIndex: 'TOUCHCASE', width: 100 },
        { text: "合約到期日", dataIndex: 'E_CODATE', width: 100 },
        { text: "付款方式", dataIndex: 'E_SOURCECODE', width: 100 },
        { text: "合約方式", dataIndex: 'M_CONTID', width: 100 },
        { text: "成本價", dataIndex: 'DISC_CPRICE', width: 100 },
        { text: "決標價", dataIndex: 'M_CONTPRICE', width: 100 },
        { text: "二次折讓數量", dataIndex: 'DISCOUNT_QTY', width: 100 },
        { text: "二次優惠單價", dataIndex: 'DISC_COST_UPRICE', width: 100 },
        { text: "聯標項次", dataIndex: 'E_ITEMARMYNO', width: 100 },
        { text: "健保價", dataIndex: 'NHI_PRICE', width: 100 },
        { text: "聯標契約數量", dataIndex: 'CONTRACTAMT', width: 100 },
        { text: "聯標項次契約總價", dataIndex: 'CONTRACTSUM', width: 100 },
        { header: "", flex: 1 }
    ],
    listeners: {
        selectionchange: function (model, records) {
            T1Rec = records.length;
            T1LastRec = records[0];
            setFormT1a();
        }
    }
});

function setFormT1a() {
    T1Grid.down('#edit').setDisabled(T1Rec === 0);
    T1Grid.down('#delete').setDisabled(T1Rec === 0);
    if (T1LastRec) {
        isNew = false;
        T1Form.loadRecord(T1LastRec);
        var f = T1Form.getForm();
        f.findField('x').setValue('U');
        T1F1 = f.findField('MIMASTHIS_SEQ').getValue();
        T1F2 = f.findField('EFFSTARTDATE_T').getValue();
        T1F3 = f.findField('MMCODE').getValue();
        var u = f.findField('MMCODE');
        u.setReadOnly(true);
        u.setFieldStyle('border: 0px');
    }
    else {
        T1Form.getForm().reset();
    }
}
function setFormT1(x, t) {
    win1.setTitle(t + T1Name);
    var f = T1Form.getForm();
    if (x === "I") {
        isNew = true;
        var r = Ext.create('T1Model');
        T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
        u = f.findField("MMCODE");
        u.setReadOnly(false);
        T1Form.getForm().findField('E_RESTRICTCODE').setValue('N');
        T1Form.getForm().findField('TRUTRATE').setValue('1');
        T1Form.getForm().findField('WARBAK').setValue('0');
        T1Form.getForm().findField('ONECOST').setValue('0');
        //T1Form.getForm().findField('E_ORDERDCFLAG').setValue('N');
        T1Form.getForm().findField('M_STOREID').setValue('0');
        T1Form.getForm().findField('ISIV').setValue('N');
        T1Form.getForm().findField('E_SOURCECODE').setValue('N');
        T1Form.getForm().findField('M_CONTID').setValue('0');
        //T1Form.getForm().findField('CONTRACTAMT').setValue('0');
        //T1Form.getForm().findField('CONTRACTSUM').setValue('0');
        T1Form.getForm().findField('TOUCHCASE').setValue('0');
        //T1Form.getForm().findField('DISCOUNT_QTY').setValue('1');
        T1Form.getForm().findField('APPQTY_TIMES').setValue('1');
        showWin1();
    }
    else {
        u = f.findField('MMCODE');
        win1.defaultFocus = u;
        if (x == "U") {
            if (T1LastRec) {
                f.loadRecord(T1LastRec);
                u = f.findField('MMCODE');
                win1.defaultFocus = u;
                showWin1();
            }
        }
        if (x == "D") {
            if (T1LastRec) {
                Ext.MessageBox.confirm('提醒', '確定刪院內碼<font color=blue>' + f.findField('MMCODE').getValue() + '</font>?', function (btn, text) {
                    if (btn === 'yes') {
                        T1Delete();
                    }
                }
                );

            }
        }
    }

    f.findField('x').setValue(x);

    //T1Form.down('#cancel').setVisible(true);
    //T1Form.down('#submit').setVisible(true);
}
function T1Submit() {
    var f = T1Form.getForm();
    if (f.isValid()) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        f.submit({
            url: T1Set,
            success: function (form, action) {
                myMask.hide();
                var f2 = T1Form.getForm();
                var r = f2.getRecord();
                hideWin1();
                switch (f2.findField("x").getValue()) {
                    case "I":
                        var v = action.result.etts[0];
                        T1Query.getForm().reset();
                        T1Query.getForm().findField('P0').setValue(v.MMCODE);
                        T1Load();
                        break;
                    case "U":
                        var v = action.result.etts[0];
                        r.set(v);
                        r.commit();
                        break;
                    case "D":
                        T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        r.commit();
                        break;
                }
                T1Load();
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

var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
    name: 'M_AGENNO',
    fieldLabel: '廠商代碼',
    limit: 20,
    queryUrl: AgennoComboGet,
    //storeAutoLoad: true,
    matchFieldWidth: false,
    listConfig: { width: 300 },
    colspan: 1,
    listeners: {
        blur: function (field, eve, eOpts) {
            //if (field.getValue() != '' && field.getValue() != null)
            //    chkAGENNO(field.getValue());
        },
        select: function (c, r, i, e) {
            var f = T1Form.getForm();
            if (r.get('AGEN_NAMEC') != '' && r.get('AGEN_NAMEC') != null) {
                f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEC'));
            }
            else if (r.get('AGEN_NAMEE') != '' && r.get('AGEN_NAMEE') != null) {
                f.findField("AGEN_NAMEC").setValue(r.get('AGEN_NAMEE'));
            }
        }
    }
});
var col_labelWid = 120;
var col_Wid = 250;
var fieldsetWid = 1200;
var T1Form = Ext.widget({
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
        //readOnly: formReadonly,
        labelAlign: 'right'
    },
    items: [
        {
            xtype: 'fieldset',
            padding: '8 0 0 8',
            autoHeight: true,
            autoWidth: true,
            //width: fieldsetWid,
            style: "margin:5px;background-color: #ecf5ff;",
            layout: 'anchor',
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
                        { xtype: 'hidden', name: 'E_CODATE' },
                        { xtype: 'hidden', name: 'EFFSTARTDATE' },

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
                                specialkey: function (f, e) {
                                    
                                    if (e.getKey() == e.ENTER && T1Form.getForm().findField('x').getValue()=='I') {
                                        Ext.Ajax.request({
                                            url: '/api/AA0159/GetMiMast',
                                            method: reqVal_p,
                                            params: {
                                                mmcode: T1Form.getForm().findField('MMCODE').getValue()
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {

                                                    if (data.etts.length > 0) {
                                                        var f = T1Form.getForm();
                                                        f.findField('M_NHIKEY').setValue(data.etts[0].M_NHIKEY);
                                                        f.findField('HEALTHOWNEXP').setValue(data.etts[0].HEALTHOWNEXP);
                                                        f.findField('DRUGSNAME').setValue(data.etts[0].DRUGSNAME);
                                                        f.findField('MMNAME_E').setValue(data.etts[0].MMNAME_E);
                                                        f.findField('MMNAME_C').setValue(data.etts[0].MMNAME_C);
                                                        f.findField('M_PHCTNCO').setValue(data.etts[0].M_PHCTNCO);
                                                        f.findField('M_ENVDT').setValue(data.etts[0].M_ENVDT);
                                                        f.findField('ISSUESUPPLY').setValue(data.etts[0].ISSUESUPPLY);
                                                        f.findField('E_MANUFACT').setValue(data.etts[0].E_MANUFACT);
                                                        f.findField('BASE_UNIT').setValue(data.etts[0].BASE_UNIT);
                                                        f.findField('M_PURUN').setValue(data.etts[0].M_PURUN);
                                                        f.findField('UNITRATE').setValue(data.etts[0].UNITRATE);
                                                        f.findField('MAT_CLASS_SUB').setValue(data.etts[0].MAT_CLASS_SUB);
                                                        f.findField('COMMON').setValue(data.etts[0].COMMON);
                                                        f.findField('TRUTRATE').setValue(data.etts[0].TRUTRATE);
                                                        f.findField('ONECOST').setValue(data.etts[0].ONECOST);
                                                        f.findField('HEALTHPAY').setValue(data.etts[0].HEALTHPAY);
                                                        f.findField('COSTKIND').setValue(data.etts[0].COSTKIND);
                                                        f.findField('E_RESTRICTCODE').setValue(data.etts[0].E_RESTRICTCODE);
                                                        f.findField('WARBAK').setValue(data.etts[0].WARBAK);
                                                        f.findField('WASTKIND').setValue(data.etts[0].WASTKIND);
                                                        f.findField('SPXFEE').setValue(data.etts[0].SPXFEE);
                                                        f.findField('ORDERKIND').setValue(data.etts[0].ORDERKIND);
                                                        f.findField('CASEDOCT').setValue(data.etts[0].CASEDOCT);
                                                        f.findField('CANCEL_ID').setValue(data.etts[0].CANCEL_ID);
                                                        f.findField('M_STOREID').setValue(data.etts[0].M_STOREID);
                                                        f.findField('DRUGKIND').setValue(data.etts[0].DRUGKIND);
                                                        f.findField('ISIV').setValue(data.etts[0].ISIV);
                                                        f.findField('SPDRUG').setValue(data.etts[0].SPDRUG);
                                                        f.findField('SPMMCODE').setValue(data.etts[0].SPMMCODE);
                                                        f.findField('DISCOUNT_QTY').setValue(data.etts[0].DISCOUNT_QTY);
                                                        f.findField('FASTDRUG').setValue(data.etts[0].FASTDRUG);
                                                        f.findField('APPQTY_TIMES').setValue(data.etts[0].APPQTY_TIMES);
                                                        f.findField('DISC_COST_UPRICE').setValue(data.etts[0].DISC_COST_UPRICE);
                                                        f.findField('M_AGENNO').setValue(data.etts[0].M_AGENNO);
                                                        f.findField('AGEN_NAMEC').setValue(data.etts[0].AGEN_NAMEC);
                                                        f.findField('M_AGENLAB').setValue(data.etts[0].M_AGENLAB);
                                                        f.findField('CASENO').setValue(data.etts[0].CASENO);
                                                        f.findField('E_SOURCECODE').setValue(data.etts[0].E_SOURCECODE);
                                                        f.findField('M_CONTID').setValue(data.etts[0].M_CONTID);
                                                        f.findField('E_CODATE_T').setValue(data.etts[0].E_CODATE_T);
                                                        f.findField('E_ITEMARMYNO').setValue(data.etts[0].E_ITEMARMYNO);
                                                        f.findField('NHI_PRICE').setValue(data.etts[0].NHI_PRICE);
                                                        f.findField('DISC_CPRICE').setValue(data.etts[0].DISC_CPRICE);
                                                        f.findField('M_CONTPRICE').setValue(data.etts[0].M_CONTPRICE);
                                                        f.findField('CONTRACTAMT').setValue(data.etts[0].CONTRACTAMT);
                                                        f.findField('CONTRACTSUM').setValue(data.etts[0].CONTRACTSUM);
                                                        f.findField('TOUCHCASE').setValue(data.etts[0].TOUCHCASE);
                                                    }
                                                }
                                            },
                                            failure: function (form, action) {
                                                
                                            }
                                        });
                                    }
                                }
                            }
                        },
                        //{
                        //    xtype: 'displayfield',
                        //    fieldLabel: '條碼代碼',
                        //    name: 'BARCODE',
                        //    readOnly: true,
                        //    colspan: 1
                        //}, {
                        //    xtype: 'displayfield',
                        //    fieldLabel: '',
                        //    name: 'MMCODE_BARCODE',
                        //    readOnly: true,
                        //    colspan: 1
                        //},
                        {
                            xtype: 'displayfield',
                            fieldLabel: '',
                            colspan: 2
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '健保代碼',
                            name: 'M_NHIKEY',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 20,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '健保自費碼',
                            name: 'HEALTHOWNEXP',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 16,
                            labelWidth: col_labelWid,
                            width: col_Wid * 2,
                            colspan: 2
                        },

                        {
                            xtype: 'textfield',
                            fieldLabel: '學名',
                            name: 'DRUGSNAME',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 300,
                            labelWidth: col_labelWid,
                            width: col_Wid * 3,
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
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '中文品名',
                            name: 'MMNAME_C',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 250,
                            labelWidth: col_labelWid,
                            width: col_Wid * 2,
                            colspan: 2
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
                            colspan: 1
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '許可證效期',
                            name: 'M_ENVDT',
                            id: 'M_ENVDT',
                            enforceMaxLength: true,
                            maxLength: 8,          //最多輸入7碼
                            maskRe: /[0-9]/,      //前端只能輸入數字
                            //regexText: '正確格式應為「YYYMMDD」，<br>例如「1080101」',
                            //regex: /^[0-9]{7}$/,
                            labelWidth: col_labelWid,
                            width: col_Wid * 2,
                            colspan: 2
                        },

                        {
                            xtype: 'textfield',
                            fieldLabel: '申請廠商',
                            name: 'ISSUESUPPLY',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 128,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '製造商',
                            name: 'E_MANUFACT',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 200,
                            labelWidth: col_labelWid,
                            width: col_Wid * 2,
                            colspan: 2
                        },

                        {
                            xtype: 'combo',
                            store: baseunitFormStore,
                            id: 'BASE_UNIT',
                            name: 'BASE_UNIT',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            fieldLabel: '藥材單位',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            colspan: 1,
                            anyMatch: true,
                            listeners: {
                                select: function (c, r, i, e) {
                                    var f = T1Form.getForm();
                                    f.findField("M_PURUN").setValue(r.get('VALUE'));
                                }
                            }
                        }, {
                            xtype: 'combo',
                            store: baseunitFormStore,
                            id: 'M_PURUN',
                            name: 'M_PURUN',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            fieldLabel: '出貨包裝單位',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            anyMatch: true,
                            matchFieldWidth: false,
                            listConfig: { width: 300 },
                            colspan: 1
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '每包裝出貨量',
                            name: 'UNITRATE',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 120,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },

                        {
                            xtype: 'combo',
                            store: matclassSubStore,
                            id: 'MAT_CLASS_SUB',
                            name: 'MAT_CLASS_SUB',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            fieldLabel: '物料子類別',
                            queryMode: 'local',
                            //readOnly: formReadonly,
                            autoSelect: true,
                            multiSelect: false,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            //allowBlank: false,
                            //fieldCls: 'required',
                            labelWidth: col_labelWid,
                            width: col_Wid,
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
                            autoSelect: true,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '與HIS單位換算比值',
                            name: 'TRUTRATE',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            minValue: 0,
                            decimalPrecision: 2, // 小數點為 0 
                            labelWidth: col_labelWid,
                            width: col_Wid,
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
                            allowBlank: false,
                            fieldCls: 'requried',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1,
                            listeners: {
                                select: function (c, r, i, e) {
                                    if (r.get('VALUE') == '1') {
                                        T1Form.getForm().findField('HEALTHPAY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                        T1Form.getForm().findField('HEALTHPAY').allowBlank = false;
                                        T1Form.getForm().findField('HEALTHPAY').fieldCls = 'required';
                                        T1Form.getForm().findField('COSTKIND').allowBlank = true;
                                        T1Form.getForm().findField('COSTKIND').fieldCls = '';
                                    }
                                    if (r.get('VALUE') == '2') {
                                        T1Form.getForm().findField('COSTKIND').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                        T1Form.getForm().findField('COSTKIND').allowBlank = false;
                                        T1Form.getForm().findField('COSTKIND').fieldCls = 'required';
                                        T1Form.getForm().findField('HEALTHPAY').allowBlank = true;
                                        T1Form.getForm().findField('HEALTHPAY').fieldCls = '';
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
                            //readOnly: formReadonly,
                            //allowBlank: false,
                            //fieldCls: 'required',
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            //readOnly: formReadonly,
                            //allowBlank: false,
                            //fieldCls: 'required',
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            allowBlank: false,
                            fieldClas: 'required',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            allowBlank: false,
                            fieldCls: 'required',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            colspan: 1
                        },

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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            colspan: 1,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                        },
                        // 20231007
                        /*{ 
                            xtype: 'combo',
                            store: form_YN,
                            id: 'E_ORDERDCFLAG',
                            name: 'E_ORDERDCFLAG',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            fieldLabel: '藥品是否停用',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            colspan: 1,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                        },*/
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
                            //readOnly: true,
                            colspan: 2
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        },
                        {
                            xtype: 'combo',
                            store: IsivStore,
                            id: 'ISIV',
                            name: 'ISIV',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            allowBlank: false,
                            fieldCls: 'required',
                            fieldLabel: '是否點滴',
                            queryMode: 'local',
                            autoSelect: true,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '特材號碼',
                            name: 'SPMMCODE',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 12,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '二次折讓數量',
                            name: 'DISCOUNT_QTY',
                            id: 'DISCOUNT_QTY',
                            enforceMaxLength: true,
                            //maxLength: 7,          //最多輸入7碼
                            maskRe: /[0-9]/,      //前端只能輸入數字
                            minValue: 0,
                            step: 1,
                            decimalPrecision: 0,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'combo',
                            store: FastDrugStore,
                            id: 'FASTDRUG',
                            name: 'FASTDRUG',
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            fieldLabel: '急救品項',
                            queryMode: 'local',
                            autoSelect: true,
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '申請倍數',
                            name: 'APPQTY_TIMES',
                            id: 'APPQTY_TIMES',
                            minValue: 1,
                            step: 1,
                            decimalPrecision: 0,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1,
                            allowBlank: false,
                            fieldCls: 'required'
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '二次優惠單價',
                            name: 'DISC_COST_UPRICE',
                            id: 'DISC_COST_UPRICE',
                            enforceMaxLength: true,
                            //maxLength: 7,          //最多輸入7碼
                            maskRe: /[0-9.-]/,       //前端只能輸入數字,
                            minValue: 0,
                            step: 1,
                            decimalPrecision: 2,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },

                        {
                            xtype: 'displayfield',
                            fieldLabel: '藥衛材基本檔修改歷程序號',
                            name: 'MIMASTHIS_SEQ',
                            readOnly: true,
                            submitValue: true,
                            colspan: 3
                        },

                        T1FormAgenno,
                        {
                            xtype: 'displayfield',
                            fieldLabel: '廠商名稱',
                            name: 'AGEN_NAMEC',
                            labelAlign: 'right',
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 2
                        },

                        {
                            xtype: 'textfield',
                            fieldLabel: '廠牌',
                            name: 'M_AGENLAB',
                            enforceMaxLength: true,
                            maxLength: 64,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '合約案號',
                            name: 'CASENO',
                            enforceMaxLength: true,
                            maxLength: 30,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 2
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '合約到期日',
                            name: 'E_CODATE_T',
                            labelAlign: 'right',
                            submitFormat: 'Xmd',
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'textfield',
                            fieldLabel: '聯標項次',
                            name: 'E_ITEMARMYNO',
                            labelAlign: 'right',
                            enforceMaxLength: true,
                            maxLength: 10,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'numberfield',
                            fieldLabel: '健保價',
                            name: 'NHI_PRICE',
                            labelAlign: 'right',
                            minValue: 0,
                            step: 1, // 每次都要 加 1 
                            decimalPrecision: 0, // 小數點為 0 
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '成本價',
                            name: 'DISC_CPRICE',
                            minValue: 0,
                            step: 1, // 每次都要 加 1 
                            decimalPrecision: 0, // 小數點為 0 
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '決標價',
                            name: 'M_CONTPRICE',
                            labelAlign: 'right',
                            minValue: 0,
                            step: 1, // 每次都要 加 1 
                            decimalPrecision: 0, // 小數點為 0 
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '聯標契約總數量',
                            name: 'CONTRACTAMT',
                            labelAlign: 'right',
                            minValue: 0,
                            step: 1, // 每次都要 加 1 
                            decimalPrecision: 0, // 小數點為 0 
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '聯標項次契約總價',
                            name: 'CONTRACTSUM',
                            labelAlign: 'right',
                            maskRe: /[0-9.]/,
                            regexText: '只能輸入數字及小數點',
                            regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        },

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
                            //readOnly: formReadonly,
                            multiSelect: false,
                            typeAhead: true, forceSelection: true, selectOnFocus: true,
                            colspan: 1
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '建立日期',
                            name: 'BEGINDATE_14_T',
                            readOnly: true,
                            submitFormat: 'Xmd',
                            submitValue: false,
                            colspan: 1
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '生效日期',
                            name: 'EFFSTARTDATE_T',
                            submitFormat: 'Xmd', //X=民國年,m=月,d=日,H=時,i=分,s=秒
                            labelAlign: 'right',
                            allowBlank: false,
                            fieldCls: 'required',
                            labelWidth: col_labelWid,
                            width: col_Wid,
                            colspan: 1
                        }


                    ]
                },
                {
                    xtype: 'button',
                    text: '儲存',
                    handler: function () {
                        if (this.up('form').getForm().isValid()) {
                            if (this.up('form').getForm().findField('MMNAME_C').getValue() == ''
                                && this.up('form').getForm().findField('MMNAME_E').getValue() == '')
                                Ext.Msg.alert('提醒', '中文品名或英文品名至少需輸入一種');
                            else {
                                var confirmSubmit = '儲存';
                                Ext.MessageBox.confirm(confirmSubmit, '是否確定儲存?', function (btn, text) {
                                    if (btn === 'yes') {
                                        T1Submit();
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
                    text: '取消',
                    handler: function () {
                        hideWin1();
                    }
                }
            ]
        }
    ]
});

// 供彈跳視窗內的呼叫以關閉彈跳視窗
closeWin = function (rtnMmcode) {
    callableWin.destroy();
    callableWin = null;
    T1Query.getForm().reset();
    T1Query.getForm().findField('P0').setValue(rtnMmcode);
    T1Tool.doRefresh();
}

var callableWin = null;
popWinForm = function (vType, strMmcode) {
    var strUrl = "AA0159_Form?strVtype=" + vType + "&strMmcode=" + strMmcode + "&strIsadm=" + isAdmin;
    if (!callableWin) {
        var popform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            height: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
            buttons: [{
                id: 'winclosed',
                disabled: false,
                text: '關閉',
                handler: function () {
                    this.up('window').destroy();
                    callableWin = null;
                }
            }]
        });
        callableWin = GetPopWin(viewport, popform, '藥品基本檔詳細資料', viewport.width - 20, viewport.height - 20);
    }
    callableWin.show();
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
        items: [T1Grid]
    }
    ]
});

var win1;
if (!win1) {
    win1 = Ext.widget('window', {
        title: '',
        closeAction: 'hide',
        width: viewport.width - 20,
        height: viewport.height - 20,
        autoScroll: true,
        resizable: false,
        items: [
            {
                itemId: 't1Form',
                region: 'north',
                items: [T1Form]
            }
        ],
        buttonAlign: 'center',
        buttons: [{
            text: '關閉',
            handler: function () {
                hideWin1()
            }
        }]
    });
}

function showWin1() {
    if (win1.hidden) { win1.show(); }
}
function hideWin1() {
    if (!win1.hidden) {
        win1.hide();
    }
}

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
                PostForm(T6GetExcelExample, p);
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
                PostForm('../../api/AA0159/GetTxt', p);
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
                            url: "/api/AA0159/SendExcel",
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
                    url: '/api/AA0159/InsertFromXls',
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
                            T1Load();
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
        { text: "生效起始時間", dataIndex: 'EFFSTARTDATE', width: 100 },
        { text: "院內碼", dataIndex: 'MMCODE', width: 100 },
        { text: "學名", dataIndex: 'DRUGSNAME', width: 100 },
        { text: "英文品名", dataIndex: 'MMNAME_E', width: 100 },
        { text: "中文品名", dataIndex: 'MMNAME_C', width: 100 },
        { text: "健保代碼", dataIndex: 'M_NHIKEY', width: 100 },
        { text: "健保自費碼", dataIndex: 'HEALTHOWNEXP', width: 100 },
        { text: "許可證號", dataIndex: 'M_PHCTNCO', width: 100 },
        { text: "許可證效期", dataIndex: 'M_ENVDT', width: 100 },
        { text: "申請廠商", dataIndex: 'ISSUESUPPLY', width: 100 },
        { text: "製造商", dataIndex: 'E_MANUFACT', width: 100 },
        { text: "藥材單位", dataIndex: 'BASE_UNIT', width: 100 },
        { text: "出貨包裝單位", dataIndex: 'M_PURUN', width: 100 },
        { text: "每包裝出貨量", dataIndex: 'UNITRATE', width: 100 },
        { text: "物料子類別", dataIndex: 'MAT_CLASS_SUB', width: 100 },
        { text: "是否常用品項", dataIndex: 'COMMON', width: 100 },
        { text: "與HIS單位換算比值", dataIndex: 'TRUTRATE', width: 100 },
        { text: "是否可單一計價", dataIndex: 'ONECOST', width: 100 },
        { text: "是否健保給付", dataIndex: 'HEALTHPAY', width: 100 },
        { text: "費用分類", dataIndex: 'COSTKIND', width: 100 },
        { text: "管制級數", dataIndex: 'E_RESTRICTCODE', width: 100 },
        { text: "是否戰備", dataIndex: 'WARBAK', width: 100 },
        { text: "是否正向消耗", dataIndex: 'WASTKIND', width: 100 },
        { text: "是否為特材", dataIndex: 'SPXFEE', width: 100 },
        { text: "採購類別", dataIndex: 'ORDERKIND', width: 100 },
        { text: "小採需求醫師", dataIndex: 'CASEDOCT', width: 100 },
        { text: "是否作廢", dataIndex: 'CANCEL_ID', width: 100 },
        { text: "庫備識別碼", dataIndex: 'M_STOREID', width: 100 },
        { text: "中西藥類別", dataIndex: 'DRUGKIND', width: 100 },
        { text: "是否點滴", dataIndex: 'ISIV', width: 100 },
        { text: "特殊品項", dataIndex: 'SPDRUG', width: 100 },
        { text: "特材號碼", dataIndex: 'SPMMCODE', width: 100 },
        { text: "急救品項", dataIndex: 'FASTDRUG', width: 100 },
        { text: "申請倍數", dataIndex: 'APPQTY_TIMES', width: 100 },
        { text: "廠商代碼", dataIndex: 'M_AGENNO', width: 100 },
        { text: "合約案號", dataIndex: 'CASENO', width: 100 },
        { text: "廠牌", dataIndex: 'M_AGENLAB', width: 100 },
        { text: "合約類別", dataIndex: 'TOUCHCASE', width: 100 },
        { text: "合約到期日", dataIndex: 'E_CODATE', width: 100 },
        { text: "付款方式", dataIndex: 'E_SOURCECODE', width: 100 },
        { text: "合約方式", dataIndex: 'M_CONTID', width: 100 },
        { text: "成本價", dataIndex: 'DISC_CPRICE', width: 100 },
        { text: "決標價", dataIndex: 'M_CONTPRICE', width: 100 },
        { text: "二次折讓數量", dataIndex: 'DISCOUNT_QTY', width: 100 },
        { text: "二次優惠單價", dataIndex: 'DISC_COST_UPRICE', width: 100 },
        { text: "聯標項次", dataIndex: 'E_ITEMARMYNO', width: 100 },
        { text: "健保價", dataIndex: 'NHI_PRICE', width: 100 },
        { text: "聯標契約數量", dataIndex: 'CONTRACTAMT', width: 100 },
        { text: "聯標項次契約總價", dataIndex: 'CONTRACTSUM', width: 100 },
        { header: "", flex: 1 }
    ]
});

var winActWidth = viewport.width - 20;
var winActHeight = viewport.height - 20;
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

showDetail = function () {
    setFormT1("U", '修改');
}


    T1Query.getForm().findField('P0').focus();

});
