Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);

Ext.onReady(function () {
    var T1Set = '';
    var MatclassComboGet = '/api/AA0158/GetMatclassCombo';
    var MatclassSubComboGet = '/api/AA0158/GetMatclassSubCombo';
    var BaseunitComboGet = '/api/AA0158/GetBaseunitCombo';
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var AgennoComboGet = '/api/AA0158/GetAgennoCombo';
    var FormEditableGet = '/api/AA0158/GetFormEditable';
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
    var T1Name = "藥衛材基本檔維護作業";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var vtype = Ext.getUrlParam('strVtype');
    var mmcode = Ext.getUrlParam('strMmcode');
    var formReadonly = true;
    var formAllowblank = true;
    var formCls = 'required';
    if (vtype == 'I' || vtype == 'U') {
        //formReadonly = false;
        formAllowblank = false;
        formCls = 'required';
        if (vtype == 'I')
            T1Set = '/api/AA0158/Create';
        else if (vtype == 'U')
            T1Set = '/api/AA0158/Update';
    }
    else if (vtype == 'V') {
        formReadonly = true;
        formAllowblank = true;
        formCls = '';
    }

    // Y/N
    var form_YN = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM'],
        data: [
            { "VALUE": "", "TEXT": "", "COMBITEM": "" },
            { "VALUE": "Y", "TEXT": "Y", "COMBITEM": "Y 作廢" },
            { "VALUE": "N", "TEXT": "N", "COMBITEM": "N 使用" }
        ]
    });

    // 物品分類代碼
    var matclassFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    // 物品分類子類別
    var matclassSubFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    // 計量單位代碼
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
            url: MatclassSubComboGet,
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
    }
    setComboData();

    var T1Store = Ext.create('WEBAPP.store.AA.AA0158VM', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0158/Get',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: mmcode
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    T1Form.loadRecord(records[0]);
                    if (records[0].data['MAT_CLSID'] == '2' || records[0].data['MAT_CLSID'] == '3') {
                        // 若為衛材,一般物品,最小撥補量為必填
                        T1Form.getForm().findField('MIN_ORDQTY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                        T1Form.getForm().findField('MIN_ORDQTY').allowBlank = false;
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

    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'M_AGENNO',
        fieldLabel: '廠商代碼',
        allowBlank: formAllowblank,
        fieldCls: formCls,
        limit: 20,
        queryUrl: AgennoComboGet,
        //storeAutoLoad: true,
        matchFieldWidth: false,
        listConfig: { width: 300 },
        colspan: 1,
        listeners: {
            blur: function (field, eve, eOpts) {
                if (field.getValue() != '' && field.getValue() != null)
                    chkAGENNO(field.getValue());
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

    //var col_labelWid = 90;
    //var col_Wid = 200;
    var col_labelWid = 120;
    var col_Wid = 250;
    var fieldsetWid = 860;

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
            readOnly: formReadonly,
            labelAlign: 'right'
        },
        items: [
            {
                xtype: 'fieldset',
                padding: '8 0 0 8',
                autoHeight: true,
                width: fieldsetWid,
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
                                {
                                    xtype: 'hidden',
                                    fieldLabel: 'Update',
                                    name: 'x',
                                },

                                {
                                    xtype: 'textfield',
                                    fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 13,
                                    //regexText: '只能輸入英文字母與數字',
                                    //regex: /^[\w-]{0,13}$/,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '條碼代碼',
                                    name: 'BARCODE',
                                    readOnly: true,
                                    colspan: 2
                                },

                                {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'CANCEL_ID',
                                    name: 'CANCEL_ID',
                                    displayField: 'COMBITEM',
                                    valueField: 'VALUE',
                                    fieldLabel: '衛材是否作廢',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    colspan: 1,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                },
                                {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_ORDERDCFLAG',
                                    name: 'E_ORDERDCFLAG',
                                    displayField: 'COMBITEM',
                                    valueField: 'VALUE',
                                    fieldLabel: '藥品是否停用',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    colspan: 2,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
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
                                    colspan: 1
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '健保自費碼',
                                    name: 'HEALTHOWNEXP',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 12,
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
                                    maxLength: 12,
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
                                    maxLength: 7,          //最多輸入7碼
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    maxLength: 120,
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
                                    maxLength: 120,
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
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
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
                                    readOnly: formReadonly,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    multiSelect: false,
                                    editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    anyMatch: true,
                                    matchFieldWidth: false,
                                    listConfig: { width: 300 },
                                    colspan: 1
                                },
                                {
                                    xtype: 'textfield',
                                    fieldLabel: '每包裝出貨量(單位/包裝)',
                                    name: 'TRUTRATE',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    maxLength: 120,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
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
                                    readOnly: formReadonly,
                                    autoSelect: true,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    //allowBlank: formAllowblank,
                                    //fieldCls: formCls,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 3,
                                    anyMatch: true,
                                    listeners: {
                                        select: function (c, r, i, e) {
                                        }
                                    }
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
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 1,
                                    listeners: {
                                        select: function (c, r, i, e) {
                                            if (r.get('VALUE') == '1') {
                                                T1Form.getForm().findField('HEALTHPAY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                                T1Form.getForm().findField('HEALTHPAY').allowBlank = false;
                                            }
                                            if (r.get('VALUE') == '2') {
                                                T1Form.getForm().findField('COSTKIND').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                                T1Form.getForm().findField('COSTKIND').allowBlank = false;
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
                                    readOnly: true,
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
                                    readOnly: true,
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
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
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    autoSelect: true,
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 3
                                },
                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '藥衛材基本檔修改歷程序號',
                                    name: 'MIMASTHIS_SEQ',
                                    readOnly: true,
                                    submitValue: false,
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
                                    maxLength: 30,
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
                                    readOnly: true,
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
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 2
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
                                    xtype: 'textfield',
                                    fieldLabel: '健保價',
                                    name: 'NHI_PRICE',
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 1
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '成本價',
                                    name: 'DISC_CPRICE',
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 1
                                },

                                {
                                    xtype: 'textfield',
                                    fieldLabel: '決標價',
                                    name: 'M_CONTPRICE',
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 1
                                },
                                {
                                    xtype: 'datefield',
                                    fieldLabel: '合約到期日',
                                    name: 'E_CODATE_T',
                                    labelAlign: 'right',
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 2
                                },

                                {
                                    xtype: 'textfield',
                                    fieldLabel: '聯標契約總數量',
                                    name: 'CONTRACTAMT',
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
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
                                    readOnly: true,
                                    multiSelect: false,
                                    typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    colspan: 1
                                },

                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '建立日期',
                                    name: 'BEGINDATE_14',
                                    readOnly: true,
                                    submitValue: false,
                                    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '生效日期',
                                    name: 'EFFSTARTDATE',
                                    readOnly: true,
                                    submitValue: false,
                                    colspan: 2
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
                                    var confirmSubmit = '';
                                    if (vtype == 'I')
                                        confirmSubmit = '新增';
                                    else if (vtype == 'U')
                                        confirmSubmit = '修改';
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
                    }
                ]
            }, {
                xtype: 'container',
                layout: 'hbox',
                items: [T2Grid]
            }

        ]
        /*,
        buttons: [
            {
                itemId: 'formsave',
                text: '儲存',
                //hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) {
                        if (this.up('form').getForm().findField('MMNAME_C').getValue() == ''
                            && this.up('form').getForm().findField('MMNAME_E').getValue() == '')
                            Ext.Msg.alert('提醒', '中文品名或英文品名至少需輸入一種');
                        else {
                            var confirmSubmit = '';
                            if (vtype == 'I')
                                confirmSubmit = '新增';
                            else if (vtype == 'U')
                                confirmSubmit = '修改';
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
                itemId: 'cancel', text: '取消',
                //hidden: true,
                handler: T1Cleanup
            }
        ]*/

    });

    function T1Cleanup() {
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "combo" || fc.xtype == "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
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
                    switch (vtype) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            setTimeout(function () {
                                parent.closeWin(T1Form.getForm().findField('MMCODE').getValue());
                            }, 100);
                            break;
                        case "U":
                            msglabel('訊息區:資料修改成功');
                            setTimeout(function () {
                                parent.closeWin(T1Form.getForm().findField('MMCODE').getValue());
                            }, 100);
                            break;
                    }
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

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'reset', text: '重設單價', disabled: true,
                handler: function () {
                    
                    
                    msglabel('訊息區:匯出完成');
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
            { name: 'EFFENDDATE', type: 'string' },
            { name: 'CANCEL_ID', type: 'string' },
            { name: 'E_ORDERDCFLAG', type: 'string' },
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
            { name: 'TRUTRATE', type: 'string' },
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
            { name: 'CONTRACTAMT', type: 'string' },
            { name: 'CONTRACTSUM', type: 'string' },
            { name: 'TOUCHCASE', type: 'string' },
            { name: 'TOUCHCASE_DESC', type: 'string' },
            { name: 'BEGINDATE_14', type: 'string' },
            { name: 'ISSPRICEDATE', type: 'string' },
            { name: 'SPDRUG', type: 'string' },
            { name: 'FASTDRUG', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'MIMASTHIS_SEQ', direction: 'ASC' }],
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
                var np = {
                    p0: mmcode
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {

            }
        }
    });
    function T2Load(loadFirst) {
        try {
            if (loadFirst) {
                T2Tool.moveFirst();
            } else {
                T2Store.load({
                    params: {
                        start: 0
                    }
                });
            }

        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, 
        plain: true,
        autoScroll: true,
        //loadingText: '處理中...',
        //loadMask: true,
        cls: 'T1',
        dockedItems: [
            /*{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Form]
            },*/
            {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer'
            },
            { text: "院內碼", dataIndex: 'MMCODE', width: 60 },
            { text: "生效起始時間", dataIndex: 'EFFSTARTDATE', width: 60 },
            { text: "生效結束時間", dataIndex: 'EFFENDDATE', width: 100 },
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
            { text: "合約到期日", dataIndex: 'E_CODATE', width: 100 },
            { text: "合約類別", dataIndex: 'TOUCHCASE_DESC', width: 100 },
            { text: "建立日期", dataIndex: 'BEGINDATE_14', width: 100 },
            { text: "單價生效日", dataIndex: 'ISSPRICEDATE', width: 100 },
            { text: "聯標契約數量", dataIndex: 'CONTRACTAMT', width: 100, style: 'text-align:left', align: 'right' },
            { text: "聯標契約總價", dataIndex: 'CONTRACTSUM', width: 100, style: 'text-align:left', align: 'right' },
            { text: "序號", dataIndex: 'MIMASTHIS_SEQ', width: 100 },
            { text: "建立日期", dataIndex: 'CREATE_TIME', width: 100},
            { text: "建立人員", dataIndex: 'CREATE_USER', width: 100 },
            { header: "", flex: 1 }
        ],
        listeners: {
        }
    });

    function chkAGENNO(parAgenno) {
        Ext.Ajax.request({
            url: GetAgennmByAgenno,
            method: reqVal_p,
            params: { agen_no: parAgenno },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length == 0) {
                        Ext.Msg.alert('訊息', '廠商代碼不存在,請重新輸入!');
                        T1Form.getForm().findField('M_AGENNO').setValue('');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
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
        items: [
            {
                itemId: 'form',
                region: 'center',
                layout: 'fit',
                //collapsible: false,
                title: '',
                border: false,
                items: [T1Form]
            }/*,
            {
                region: 'sourth',
                layout: 'fit',
                //collapsible: false,
                title: '',
                split: true,
                items: [T2Grid]
            }*/
        ]
    });

    if (vtype == 'I') {
        // 代入預設值
        var f = T1Form.getForm();
        f.findField('MAT_CLASS').setValue('02');
        f.findField('M_APPLYID').setValue('Y');
        f.findField('CANCEL_ID').setValue('N');
        f.findField('WEXP_ID').setValue('N');
        f.findField('WLOC_ID').setValue('N');
    }
    else if (vtype == 'V' || vtype == 'U') {
        T1Load();
        T2Load();

    }

    function everyFormEditable() {
        T1Form.getForm().getFields().each(function (fc) {
            fc.setReadOnly(false)
        });
    }

    function chkFormEditable() {
        Ext.Ajax.request({
            url: FormEditableGet,
            method: reqVal_p,
            params: { p0: mmcode },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            if (T1Form.getForm().findField(tb_data[i].VALUE)) {
                                T1Form.getForm().findField(tb_data[i].VALUE).setReadOnly(false);
                                T1Form.getForm().findField(tb_data[i].VALUE).setFieldStyle('{color:black; border:0; background-color:#9ec9ff; background-image:none;}');
                            }
                        }
                    }
                    else {
                        // 若查無可編輯欄位,代表於HIS轉入TABLE無對應資料
                        everyFormEditable();
                        T1Form.getForm().findField('MMCODE').setReadOnly(true);
                    }
                    if (vtype == 'U') {
                        T1Form.getForm().findField('MAT_CLASS').setReadOnly(true); // 物料分類代碼不能改
                        T1Form.getForm().findField('MIN_ORDQTY').setReadOnly(false); // 最小撥補量可以修改

                        
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    
    if (vtype == 'I' || vtype == 'U') {
        if (isAdmin == 'Y') {
            // 為Admin則可修改所有欄位
            everyFormEditable();
            if (vtype == 'U')
                T1Form.getForm().findField('MMCODE').setReadOnly(true); // 但院內碼不能改
        }
        else {
            // 修改時只能異動指定欄位
            if (vtype == 'U')
                chkFormEditable();
            else if (vtype == 'I')
                everyFormEditable();
        }
        if (vtype == 'U') {
            T1Form.getForm().findField('MAT_CLASS').setReadOnly(true); // 物料分類代碼不能改
            //T1Form.getForm().findField('BASE_UNIT').setReadOnly(false);
        }
        
    }
});