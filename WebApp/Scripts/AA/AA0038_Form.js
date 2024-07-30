Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.ImageGridField']);

Ext.onReady(function () {
    var T1Set = '';
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';
    var BaseunitComboGet = '/api/AA0038/GetBaseunitCombo';
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno';
    var AgennoComboGet = '/api/AA0038/GetAgennoCombo';
    var FormEditableGet = '/api/AA0038/GetFormEditable';
    var ESourceCodeComboGet = '/api/AA0038/GetESourceCodeCombo';
    var T1Name = "衛材基本檔維護作業";

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var vtype = Ext.getUrlParam('strVtype');
    var mmcode = Ext.getUrlParam('strMmcode');
    var isAdmin = Ext.getUrlParam('strIsadm');
    var fid = Ext.getUrlParam('strFid');
    var formReadonly = true;
    var formAllowblank = true;
    var formCls = 'required';
    if (vtype == 'I' || vtype == 'U')
    {
        //formReadonly = false;
        formAllowblank = false;
        formCls = 'required';
        if (vtype == 'I')
            T1Set = '/api/AA0038/Create';
        else if (vtype == 'U')
            T1Set = '/api/AA0038/Update';
    }
    else if (vtype == 'V')
    {
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
    // 來源代碼代碼
    var eSourceCodeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MatclassComboGet,
            method: reqVal_p,
            params: {
                p0: fid,
                p1: vtype
            },
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
    }
    setComboData();

    var T1Store = Ext.create('WEBAPP.store.MiMastMaintain', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0038/Get',
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
                    if (records[0].data['MAT_CLSID'] == '2' || records[0].data['MAT_CLSID'] == '3')
                    {
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

    var col_labelWid = 90;
    var col_Wid = 200;
    var fieldsetWid = 860;
    var cbMstoreid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_STOREID',
        id: 'M_STOREID',
        fieldLabel: '庫備識別碼',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_STOREID'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMcontid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_CONTID',
        id: 'M_CONTID',
        fieldLabel: '合約識別碼',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_CONTID'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        autoSelect: true,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 150 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMconsumid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_CONSUMID',
        id: 'M_CONSUMID',
        fieldLabel: '消耗屬性',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_CONSUMID'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMpaykind = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_PAYKIND',
        id: 'M_PAYKIND',
        fieldLabel: '給付類別',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_PAYKIND'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        matchFieldWidth: false,
        listConfig: { width: 210 },
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMpayid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_PAYID',
        id: 'M_PAYID',
        fieldLabel: '計費方式',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_PAYID'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMtrnid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_TRNID',
        id: 'M_TRNID',
        fieldLabel: '盤差種類',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_TRNID'
        },
        width: col_Wid,
        labelWidth: col_labelWid,
        colspan: 2,
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
    var cbMapplyid = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'M_APPLYID',
        id: 'M_APPLYID',
        fieldLabel: '申請申購識別碼',
        queryParam: {
            GRP_CODE: 'MI_MAST',
            DATA_NAME: 'M_APPLYID'
        },
        width: col_Wid,
        labelWidth: col_labelWid + 10,
        colspan: 1,
        matchFieldWidth: false,
        listConfig: { width: 210 },
        allowBlank: formAllowblank,
        fieldCls: formCls,
        autoSelect: true,
        readOnly: formReadonly,
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
        width: col_Wid,
        labelWidth: col_labelWid,
        colspan: 1,
        matchFieldWidth: false,
        listConfig: { width: 210 },
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
        width: col_Wid,
        labelWidth: col_labelWid,
        colspan: 2,
        matchFieldWidth: false,
        listConfig: { width: 210 },
        insertEmptyRow: true,
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    });
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
    var cbContracno = Ext.create('WEBAPP.form.ParamCombo', {
        name: 'CONTRACNO',
        id: 'CONTRACNO',
        hidden: true,
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
                            columns: 4
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
                                regex: /^[\w-]{0,13}$/,
                                allowBlank: formAllowblank,
                                fieldCls: formCls,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1
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
                                width: col_Wid,
                                colspan: 1,
                                anyMatch: true,
                                listeners: {
                                    select: function (c, r, i, e) {
                                        // 一般物品
                                        if (r.get('VALUE') == '02' || r.get('VALUE') == '03' || r.get('VALUE') == '04' || r.get('VALUE') == '05'
                                            || r.get('VALUE') == '06' || r.get('VALUE') == '07' || r.get('VALUE') == '08')
                                        {
                                            var f = T1Form.getForm();
                                            if (f.findField('MIN_ORDQTY').getValue() == '' || f.findField('MIN_ORDQTY').getValue() == null)
                                                f.findField('MIN_ORDQTY').setValue('1'); // 最小撥補量預設1
                                            // 若為衛材,一般物品,最小撥補量為必填
                                            T1Form.getForm().findField('MIN_ORDQTY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                            T1Form.getForm().findField('MIN_ORDQTY').allowBlank = false;
                                        }

                                        // 一般物品,能設,通信的優惠合約單價,最小數量,優惠最小單價不能直接維護,而由合約單價和廠商包裝轉換率計算
                                        // 2020.05.19增加衛材也套用此規則
                                        if (fid == 'ED' || fid == 'CN' || fid == 'M' || T1Form.getForm().findField('MAT_CLASS').getValue() == '02') {
                                            T1Form.getForm().findField('UPRICE').setValue('');
                                            T1Form.getForm().findField('DISC_CPRICE').setValue('');
                                            T1Form.getForm().findField('DISC_UPRICE').setValue('');
                                            T1Form.getForm().findField('UPRICE').setReadOnly(true);
                                            T1Form.getForm().findField('DISC_CPRICE').setReadOnly(true);
                                            T1Form.getForm().findField('DISC_UPRICE').setReadOnly(true);
                                            // 折讓比為必填
                                            T1Form.getForm().findField('M_DISCPERC').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                                            T1Form.getForm().findField('M_DISCPERC').allowBlank = false;
                                            if (T1Form.getForm().findField('M_DISCPERC').getValue() == '' || T1Form.getForm().findField('M_DISCPERC').getValue() == null)
                                                T1Form.getForm().findField('M_DISCPERC').setValue('0');
                                        }
                                        else
                                        {
                                            T1Form.getForm().findField('UPRICE').setReadOnly(false);
                                            T1Form.getForm().findField('DISC_CPRICE').setReadOnly(false);
                                            T1Form.getForm().findField('DISC_UPRICE').setReadOnly(false);
                                            // 折讓比非必填
                                            T1Form.getForm().findField('M_DISCPERC').setFieldStyle('{color:black; border:0; background-color:#ffffff; background-image:none;}');
                                            T1Form.getForm().findField('M_DISCPERC').allowBlank = true;
                                        }
                                    }
                                }
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
                            }, cbCancelnote, {
                                xtype: 'textfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 250,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 250,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '院內簡稱',
                                name: 'EASYNAME',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 60,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4
                            }, cbMpaykind, cbMpayid, cbMtrnid, cbMapplyid, cbWexpid, cbWlocid, cbMstoreid, cbMcontid, cbMconsumid, {
                                xtype: 'textfield',
                                fieldLabel: '最小撥補量',
                                name: 'MIN_ORDQTY',
                                colspan: 2,
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'combo',
                                store: eSourceCodeStore,
                                id: 'E_SOURCECODE',
                                name: 'E_SOURCECODE',
                                displayField: 'COMBITEM',
                                valueField: 'VALUE',
                                fieldLabel: '來源代碼',
                                queryMode: 'local',
                                autoSelect: true,
                                readOnly: true,
                                multiSelect: false,
                                typeAhead: true, forceSelection: true, selectOnFocus: true,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '環保或衛署許可證',
                                name: 'M_PHCTNCO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 120,
                                labelWidth: col_labelWid + 20,
                                width: col_Wid * 2,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '環保證號效期',
                                name: 'M_ENVDT',
                                id: 'M_ENVDT',
                                enforceMaxLength: true,
                                maxLength: 7,          //最多輸入7碼
                                maskRe: /[0-9]/,      //前端只能輸入數字
                                regexText: '正確格式應為「YYYMMDD」，<br>例如「1080101」',
                                regex: /^[0-9]{7}$/,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '健保碼',
                                name: 'M_NHIKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 12,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '健保給付價',
                                name: 'NHI_PRICE',
                                colspan: 3,
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '聯標項次/院辦案號',
                                name: 'E_ITEMARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid + 20,
                                width: col_Wid,
                                colspan: 1
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '合約年度',
                                name: 'E_YRARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '聯標組別',
                                name: 'E_CLFARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 2
                            }, {
                                xtype: 'combo',
                                store: form_YN,
                                id: 'M_SUPPLYID',
                                name: 'M_SUPPLYID',
                                displayField: 'TEXT',
                                valueField: 'VALUE',
                                fieldLabel: '是否供應契約',
                                queryMode: 'local',
                                autoSelect: true,
                                readOnly: formReadonly,
                                multiSelect: false,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                colspan: 4
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠商名稱',
                                name: 'AGEN_NAMEC',
                                labelAlign: 'right',
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1
                            }, T1FormAgenno, {
                                xtype: 'textfield',
                                fieldLabel: '廠牌',
                                name: 'M_AGENLAB',
                                enforceMaxLength: true,
                                maxLength: 30,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 2
                            }, {
                                xtype: 'combo',
                                store: baseunitFormStore,
                                id: 'M_PURUN',
                                name: 'M_PURUN',
                                displayField: 'COMBITEM',
                                valueField: 'VALUE',
                                fieldLabel: '申購計量單位',
                                queryMode: 'local',
                                autoSelect: true,
                                readOnly: formReadonly,
                                allowBlank: formAllowblank,
                                fieldCls: formCls,
                                multiSelect: false,
                                editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                anyMatch: true,
                                matchFieldWidth: false,
                                listConfig: { width: 300 }
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '合約單價',
                                name: 'M_CONTPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '優惠合約單價',
                                name: 'DISC_CPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '折讓比',
                                name: 'M_DISCPERC',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '最小單價',
                                name: 'UPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '優惠最小單價',
                                name: 'DISC_UPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '廠商包裝轉換率',
                                name: 'EXCH_RATIO',
                                labelAlign: 'right',
                                allowBlank: formAllowblank,
                                fieldCls: formCls,
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 1
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
                                editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                allowBlank: formAllowblank,
                                fieldCls: formCls,
                                colspan: 3,
                                anyMatch: true,
                                listeners: {
                                    select: function (c, r, i, e) {
                                        var f = T1Form.getForm();
                                        f.findField("M_PURUN").setValue(r.get('VALUE'));
                                    }
                                }
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '長度(CM)',
                                name: 'M_VOLL',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '寬度(CM)',
                                name: 'M_VOLW',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '高度(CM)',
                                name: 'M_VOLH',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '圓周',
                                name: 'M_VOLC',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '材積轉換率',
                                name: 'M_SWAP',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '自費價',
                                name: 'HOSP_PRICE',
                                labelAlign: 'right',
                                colspan: 2
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
                                xtype: 'imagegrid',
                                fieldLabel: '附加文件',
                                name: 'PFILE_ID',
                                colspan: 4,
                                width: col_Wid * 4,
                                height: 150
                            },
                            {
                                xtype: 'textfield',
                                fieldLabel: 'ID碼',
                                hidden: true,
                                name: 'M_IDKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 8
                            },
                            {
                                xtype: 'textfield',
                                fieldLabel: '衛材料號碼',
                                hidden: true,
                                name: 'M_INVKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 16
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '行政院碼',
                                hidden: true,
                                name: 'M_GOVKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 12,
                                labelWidth: col_labelWid,
                                width: col_Wid * 2,
                                colspan: 2
                            }, {
                                xtype: 'combo',
                                store: form_YN,
                                hidden: true,
                                id: 'M_MATID',
                                name: 'M_MATID',
                                displayField: 'TEXT',
                                valueField: 'VALUE',
                                fieldLabel: '是否聯標',
                                queryMode: 'local',
                                autoSelect: true,
                                readOnly: formReadonly,
                                multiSelect: false,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
                            }, cbContracno, {
                                xtype: 'hidden',
                                fieldLabel: 'Update',
                                name: 'x',
                            }, {
                                xtype: 'hidden',
                                fieldLabel: 'MAT_CLSID',
                                name: 'MAT_CLSID',
                                submitValue: true
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

    if (vtype == 'I')
    {
        // 代入預設值
        var f = T1Form.getForm();
        f.findField('MAT_CLASS').setValue('02');
        f.findField('M_APPLYID').setValue('Y');
        f.findField('CANCEL_ID').setValue('N');
        f.findField('WEXP_ID').setValue('N');
        f.findField('WLOC_ID').setValue('N');
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
                    else
                    {
                        // 若查無可編輯欄位,代表於HIS轉入TABLE無對應資料
                        everyFormEditable();
                        T1Form.getForm().findField('MMCODE').setReadOnly(true);  
                    }
                    if (vtype == 'U')
                    {
                        T1Form.getForm().findField('MAT_CLASS').setReadOnly(true); // 物料分類代碼不能改
                        T1Form.getForm().findField('MIN_ORDQTY').setReadOnly(false); // 最小撥補量可以修改

                        if (fid == 'ED' || fid == 'CN' || fid == 'M' || T1Form.getForm().findField('MAT_CLASS').getValue() == '02')
                        {
                            // 一般物品,能設,通信的優惠合約單價,最小數量,優惠最小單價不能直接維護,而由合約單價和廠商包裝轉換率計算
                            // 2020.05.19增加衛材也套用此規則
                            T1Form.getForm().findField('UPRICE').setReadOnly(true);
                            T1Form.getForm().findField('DISC_CPRICE').setReadOnly(true);
                            T1Form.getForm().findField('DISC_UPRICE').setReadOnly(true);
                        }
                    }    
                }
            },
            failure: function (response, options) {

            }
        });
    }

    if (fid == 'ED' || fid == 'CN') {
        // 能設/通信不用最小撥補量
        T1Form.getForm().findField('MIN_ORDQTY').setVisible(false);
        if (vtype == 'I')
        {
            var f = T1Form.getForm();
            f.findField('M_TRNID').setValue('1');
            f.findField('M_CONSUMID').setValue('1');
        }
    }
    if (vtype == 'I' || vtype == 'U')
    {
        if (isAdmin == 'Y')
        {
            // 為Admin則可修改所有欄位
            everyFormEditable();
            if (vtype == 'U')
                T1Form.getForm().findField('MMCODE').setReadOnly(true); // 但院內碼不能改
        }
        else
        {
            // 修改時只能異動指定欄位
            if (vtype == 'U')
                chkFormEditable();
            else if (vtype == 'I')
                everyFormEditable();
        }
        if (vtype == 'U')
        {
            T1Form.getForm().findField('MAT_CLASS').setReadOnly(true); // 物料分類代碼不能改
            //T1Form.getForm().findField('BASE_UNIT').setReadOnly(false);
        }
            
        T1Form.getForm().findField('PFILE_ID').setReadOnly(false);
        T1Form.down('#formsave').setVisible(true);
    }
    else
        T1Form.getForm().findField('PFILE_ID').setReadOnly(true);
});