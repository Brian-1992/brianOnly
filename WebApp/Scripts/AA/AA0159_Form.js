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
    var BaseunitComboGet = '/api/AA0158/GetBaseunitCombo';
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var AgennoComboGet = '/api/AA0158/GetAgennoCombo';
    var FormEditableGet = '/api/AA0158/GetFormEditable';
    var T1Name = "藥品基本檔維護作業";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var vtype = Ext.getUrlParam('strVtype');
    var mmcode = Ext.getUrlParam('strMmcode');
    var isAdmin = Ext.getUrlParam('strIsadm');
    var formReadonly = true;
    var formAllowblank = true;
    var formCls = 'required';
    if (vtype == 'I' || vtype == 'U') {
        // formReadonly = false;
        formAllowblank = false;
        formCls = 'required';
        if (vtype == 'I')
            T1Set = '/api/AA0159/Create';
        else if (vtype == 'U')
            T1Set = '/api/AA0159/Update';
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
    // 計量單位代碼
    var baseunitFormStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,
            params: { p0: 'E' },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            matclassFormStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                        if (vtype == 'I')
                            T1Form.getForm().findField('MAT_CLASS').setValue('01');
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
    }
    setComboData();

    var T1Store = Ext.create('WEBAPP.store.MiMastMaintain', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0159/Get',
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
                    if (records[0].data.E_CODATE != null) {
                        var e_codate = records[0].data['E_CODATE'].toString();
                        if (e_codate.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || e_codate.substring(0, 24) == 'Mon Jan 01 0001 00:00:00')
                            records[0].data['E_CODATE'] = '';  // 若資料庫值為null,在這邊另外做清除
                    }
                    T1Form.loadRecord(records[0]);
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

    var col_labelWid = 90;
    var col_Wid = 180;
    var fieldsetWid = 780;
    var cbEsupstatus = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_SUPSTATUS',
        id: 'E_SUPSTATUS',
        fieldLabel: '廠商供應狀況',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_SUPSTATUS'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEifpublic = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_IFPUBLIC',
        id: 'E_IFPUBLIC',
        fieldLabel: '是否公藥',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_IFPUBLIC'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEstocktype = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_STOCKTYPE',
        id: 'E_STOCKTYPE',
        fieldLabel: '扣庫規則分類',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_STOCKTYPE'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEgparmyno = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_GPARMYNO',
        id: 'E_GPARMYNO',
        fieldLabel: '軍聯項次分類',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_GPARMYNO'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEprescriptype = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_PRESCRIPTYPE',
        id: 'E_PRESCRIPTYPE',
        fieldLabel: '藥品單複方',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_PRESCRIPTYPE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEdrugclass = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_DRUGCLASS',
        id: 'E_DRUGCLASS',
        fieldLabel: '用藥類別',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_DRUGCLASS'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEdrugclassify = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_DRUGCLASSIFY',
        id: 'E_DRUGCLASSIFY',
        fieldLabel: '藥品性質',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_DRUGCLASSIFY'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEcomitcode = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_COMITCODE',
        id: 'E_COMITCODE',
        fieldLabel: '藥委會品項',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_COMITCODE'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEinvflag = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_INVFLAG',
        id: 'E_INVFLAG',
        fieldLabel: '是否盤點品項',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_INVFLAG'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEpurtype = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_PURTYPE',
        id: 'E_PURTYPE',
        fieldLabel: '藥品採購案別',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_PURTYPE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEsourcecode = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_SOURCECODE',
        id: 'E_SOURCECODE',
        fieldLabel: '來源代碼',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_SOURCECODE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEdrugapltype = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_DRUGAPLTYPE',
        id: 'E_DRUGAPLTYPE',
        fieldLabel: '藥品請領類別',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_DRUGAPLTYPE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbEparcode = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_PARCODE',
        id: 'E_PARCODE',
        fieldLabel: '母藥註記',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_PARCODE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 100 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbWexpid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'WEXP_ID',
        id: 'WEXP_ID',
        fieldLabel: '批號效期註記',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'WEXP_ID'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbWlocid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'WLOC_ID',
        id: 'WLOC_ID',
        fieldLabel: '儲位記錄註記',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'WLOC_ID'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbErestrictcode = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'E_RESTRICTCODE',
        id: 'E_RESTRICTCODE',
        fieldLabel: '管制用藥',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'E_RESTRICTCODE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        insertEmptyRow: false,
        autoSelect: true,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 160 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbContracno = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'CONTRACNO',
        id: 'CONTRACNO',
        fieldLabel: '合約碼',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'CONTRACNO'
        },
        width: col_Wid * 2,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 300 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbCancelnote = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'CANCEL_NOTE',
        id: 'CANCEL_NOTE',
        fieldLabel: '作廢備註',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'CANCEL_NOTE'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        colspan: 1,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 160 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'M_AGENNO',
        fieldLabel: '廠商代碼',
        allowBlank: formAllowblank,
        fieldCls: formCls,
        limit: 20,
        queryUrl: AgennoComboGet,
        //storeAutoLoad: true,
        matchFieldWidth: false,
        listConfig: { width: 300 },
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
                            columns: 4,
                            width: 900,
                        },
                        items:
                            [
                                {
                                    xtype: 'textfield',
                                    fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 13,
                                    regexText: '只能輸入英文字母與數字',
                                    regex: /^[\w]{0,13}$/,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 4
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
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '英文品名',
                                    name: 'MMNAME_E',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 250,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '院內簡稱',
                                    name: 'EASYNAME',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 60,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'combo',
                                    store: matclassFormStore,
                                    id: 'MAT_CLASS',
                                    name: 'MAT_CLASS',
                                    displayField: 'COMBITEM',
                                    valueField: 'VALUE',
                                    fieldLabel: '物料分類代碼',
                                    queryMode: 'local',
                                    readOnly: formReadonly,
                                    autoSelect: true,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2,
                                    anyMatch: true
                                }, {
                                    xtype: 'combo',
                                    store: baseunitFormStore,
                                    id: 'BASE_UNIT',
                                    name: 'BASE_UNIT',
                                    displayField: 'COMBITEM',
                                    valueField: 'VALUE',
                                    fieldLabel: '計量單位代碼',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2,
                                    anyMatch: true,
                                    listeners: {
                                        select: function (c, r, i, e) {
                                            var f = T1Form.getForm();
                                            f.findField("M_PURUN").setValue(r.get('VALUE'));
                                        }
                                    }
                                }, cbEsupstatus, {
                                    xtype: 'textfield',
                                    fieldLabel: '原製造商',
                                    name: 'E_MANUFACT',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 30,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2,
                                }, cbEifpublic, cbEstocktype, {
                                    xtype: 'textfield',
                                    fieldLabel: '劑量',
                                    name: 'E_SPECNUNIT',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 30,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '成份',
                                    name: 'E_COMPUNIT',
                                    labelAlign: 'right',
                                    enforceMaxLength: true,
                                    maxLength: 30,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '軍聯項次年號',
                                    name: 'E_YRARMYNO',
                                    enforceMaxLength: true,
                                    maxLength: 10,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '軍聯項次號',
                                    name: 'E_ITEMARMYNO',
                                    enforceMaxLength: true,
                                    maxLength: 10,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, cbEgparmyno, {
                                    xtype: 'textfield',
                                    fieldLabel: '軍聯項次組別',
                                    name: 'E_CLFARMYNO',
                                    enforceMaxLength: true,
                                    maxLength: 10,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'datefield',
                                    id: 'E_CODATE',
                                    name: 'E_CODATE',
                                    fieldLabel: '合約效期',
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, cbEprescriptype, cbEdrugclass, cbEdrugclassify, {
                                    xtype: 'textfield',
                                    fieldLabel: '藥品劑型',
                                    name: 'E_DRUGFORM',
                                    enforceMaxLength: true,
                                    maxLength: 50,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '藥委會註記',
                                    name: 'E_COMITMEMO',
                                    enforceMaxLength: true,
                                    maxLength: 256,
                                    labelWidth: col_labelWid,
                                    width: col_Wid * 2,
                                    colspan: 2
                                }, cbEcomitcode, cbEinvflag, cbEpurtype, cbWexpid, cbWlocid, cbEsourcecode, cbEdrugapltype, {
                                    xtype: 'textfield',
                                    fieldLabel: '軍品院內碼',
                                    name: 'E_ARMYORDCODE',
                                    enforceMaxLength: true,
                                    maxLength: 30,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, cbEparcode, {
                                    xtype: 'textfield',
                                    fieldLabel: '母藥院內碼',
                                    name: 'E_PARORDCODE',
                                    enforceMaxLength: true,
                                    maxLength: 10,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '子藥轉換量',
                                    name: 'E_SONTRANSQTY',
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    enforceMaxLength: true,
                                    maskRe: /[0-9]/,      //前端只能輸入數字
                                    maxLength: 4,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, cbErestrictcode, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_ORDERDCFLAG',
                                    name: 'E_ORDERDCFLAG',
                                    displayField: 'VALUE',
                                    valueField: 'VALUE',
                                    fieldLabel: '停用碼',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_HIGHPRICEFLAG',
                                    name: 'E_HIGHPRICEFLAG',
                                    displayField: 'VALUE',
                                    valueField: 'VALUE',
                                    fieldLabel: '高價用藥',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_RETURNDRUGFLAG',
                                    name: 'E_RETURNDRUGFLAG',
                                    displayField: 'VALUE',
                                    valueField: 'VALUE',
                                    fieldLabel: '合理回流藥',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_RESEARCHDRUGFLAG',
                                    name: 'E_RESEARCHDRUGFLAG',
                                    displayField: 'VALUE',
                                    valueField: 'VALUE',
                                    fieldLabel: '研究用藥',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'E_VACCINE',
                                    name: 'E_VACCINE',
                                    displayField: 'VALUE',
                                    valueField: 'VALUE',
                                    fieldLabel: '疫苗',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '服用藥別',
                                    name: 'E_TAKEKIND',
                                    enforceMaxLength: true,
                                    maxLength: 2,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, {
                                    xtype: 'combo',
                                    store: form_YN,
                                    id: 'CANCEL_ID',
                                    name: 'CANCEL_ID',
                                    displayField: 'COMBITEM',
                                    valueField: 'VALUE',
                                    fieldLabel: '是否作廢',
                                    queryMode: 'local',
                                    autoSelect: true,
                                    readOnly: formReadonly,
                                    multiSelect: false,
                                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '院內給藥途徑',
                                    name: 'E_PATHNO',
                                    enforceMaxLength: true,
                                    maxLength: 6,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '醫囑單位',
                                    name: 'E_ORDERUNIT',
                                    enforceMaxLength: true,
                                    maxLength: 6,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '門診給藥頻率',
                                    name: 'E_FREQNOO',
                                    enforceMaxLength: true,
                                    maxLength: 15,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '住院給藥頻率',
                                    name: 'E_FREQNOI',
                                    enforceMaxLength: true,
                                    maxLength: 15,
                                    labelWidth: col_labelWid,
                                    width: col_Wid
                                }, cbContracno, {
                                    xtype: 'displayfield',
                                    fieldLabel: '廠商名稱',
                                    name: 'AGEN_NAMEC',
                                    labelAlign: 'right',
                                    labelWidth: col_labelWid,
                                    width: col_Wid,
                                    colspan: 1
                                }, cbAgenno, {
                                    xtype: 'textfield',
                                    fieldLabel: '最小單價',
                                    name: 'UPRICE',
                                    labelAlign: 'right',
                                    //maskRe: /[0-9.]/,
                                    //regexText: '只能輸入數字及小數點',
                                    //regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '合約單價',
                                    name: 'M_CONTPRICE',
                                    labelAlign: 'right',
                                    //maskRe: /[0-9.]/,
                                    //regexText: '只能輸入數字及小數點',
                                    //regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '優惠合約單價',
                                    name: 'DISC_CPRICE',
                                    labelAlign: 'right',
                                    //maskRe: /[0-9.]/,
                                    //regexText: '只能輸入數字及小數點',
                                    //regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '折讓比',
                                    name: 'M_DISCPERC',
                                    labelAlign: 'right',
                                    //maskRe: /[0-9.]/,
                                    //regexText: '只能輸入數字及小數點',
                                    //regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '優惠最小單價',
                                    name: 'DISC_UPRICE',
                                    colspan: 1,
                                    labelAlign: 'right',
                                    //maskRe: /[0-9.]/,
                                    //regexText: '只能輸入數字及小數點',
                                    //regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '廠商包裝轉換率',
                                    name: 'EXCH_RATIO',
                                    allowBlank: formAllowblank,
                                    fieldCls: formCls,
                                    colspan: 1,
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([1-9]))(\.[0-9]+)?$|^0(\.[0-9]+)$/
                                }, cbCancelnote, {
                                    xtype: 'textfield',
                                    fieldLabel: '健保給付價',
                                    name: 'NHI_PRICE',
                                    labelAlign: 'right',
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字及小數點',
                                    regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '自費價',
                                    name: 'HOSP_PRICE',
                                    labelAlign: 'right'
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '住院扣庫轉換量',
                                    name: 'E_STOCKTRANSQTYI',
                                    labelAlign: 'right'
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '生效起日',
                                    name: 'BEGINDATE',
                                    readOnly: true,
                                    submitValue: false
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '生效迄日',
                                    name: 'ENDDATE',
                                    readOnly: true,
                                    submitValue: false,
                                    colspan: 3
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '成份名稱',
                                    name: 'E_SCIENTIFICNAME',
                                    labelAlign: 'right',
                                    colspan: 3
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '健保碼',
                                    name: 'M_NHIKEY',
                                    labelAlign: 'right'
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '單次訂購達優惠數量折讓意願',
                                    name: 'ISWILLING',
                                    labelAlign: 'right',
                                    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '單次採購優惠數量',
                                    name: 'DISCOUNT_QTY',
                                    labelAlign: 'right',
                                    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '單次訂購達優惠數量成本價',
                                    name: 'DISC_COST_UPRICE',
                                    labelAlign: 'right',
                                    colspan: 2
                                    //}, {
                                    //    xtype: 'displayfield',
                                    //    fieldLabel: '標案來源',
                                    //    name: 'SELF_BID_SOURCE',
                                    //    labelAlign: 'right',
                                    //    maxlength: 10,
                                    //    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '採購上限金額',
                                    name: 'SELF_PUR_UPPER_LIMIT',
                                    labelAlign: 'right',
                                    //width: col_Wid * 2,
                                    colspan: 1
                                },
                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '合約案號',
                                    name: 'SELF_CONTRACT_NO',
                                    labelAlign: 'right',
                                    //width: col_Wid * 2,
                                    maxlength: 20,
                                    colspan: 1
                                },
                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '合約生效起日',
                                    name: 'SELF_CONT_BDATE',
                                    labelAlign: 'right',
                                    maxlength: 7,
                                    colspan: 1
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '合約生效迄日',
                                    name: 'SELF_CONT_EDATE',
                                    labelAlign: 'right',
                                    maxlength: 7,
                                    colspan: 1
                                }, {
                                    xtype: 'imagegrid',
                                    fieldLabel: '附加文件',
                                    name: 'PFILE_ID',
                                    readOnly: formReadonly,
                                    colspan: 4,
                                    width: col_Wid * 4,
                                    height: 150
                                }, {
                                    xtype: 'hidden',
                                    fieldLabel: 'Update',
                                    name: 'x',
                                }, {
                                    xtype: 'hidden',
                                    submitValue: true,
                                    name: 'M_PURUN'
                                }
                            ]
                    }
                ]
            }],
        buttons: [{
            itemId: 'formsave',
            text: '儲存',
            hidden: true,
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
        }]
    });

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
        items: [{
            itemId: 'form',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }]
    });

    if (vtype == 'I') {
        // 代入預設值
        var f = T1Form.getForm();
        f.findField('CANCEL_ID').setValue('N');
        f.findField('WEXP_ID').setValue('Y');
        f.findField('WLOC_ID').setValue('N');
        f.findField('E_RESTRICTCODE').setValue('N');
        f.findField('E_ORDERDCFLAG').setValue('N');
        f.findField('E_HIGHPRICEFLAG').setValue('N');
        f.findField('E_RETURNDRUGFLAG').setValue('N');
        f.findField('E_RESEARCHDRUGFLAG').setValue('N');
        f.findField('E_VACCINE').setValue('N');
        f.findField('E_TAKEKIND').setValue('12');
    }
    else if (vtype == 'V' || vtype == 'U')
        T1Load();

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
            // 非Admin不能新增,修改時只能異動指定欄位
            if (vtype == 'U')
                chkFormEditable();
        }
        //優惠合約單價,合約單價,最小數量,優惠最小單價,折讓比不能直接維護,只由HIS轉入
        T1Form.getForm().findField('M_CONTPRICE').setReadOnly(true);
        T1Form.getForm().findField('UPRICE').setReadOnly(true);
        T1Form.getForm().findField('DISC_CPRICE').setReadOnly(true);
        T1Form.getForm().findField('DISC_UPRICE').setReadOnly(true);
        T1Form.getForm().findField('M_DISCPERC').setReadOnly(true);

        T1Form.getForm().findField('PFILE_ID').setReadOnly(false);
        T1Form.down('#formsave').setVisible(true);
    }
    else
        T1Form.getForm().findField('PFILE_ID').setReadOnly(true);

});