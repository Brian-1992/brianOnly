Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var MmcodeComboGet = '/api/AA0125/GetMmcodeCombo';       //院內碼
    var MatclassComboGet = '/api/AA0038/GetMatclassCombo';   //物料分類
    var BaseunitComboGet = '/api/AA0038/GetBaseunitCombo';   //單位計量代碼
    var GetAgennmByAgenno = '/api/BC0002/GetAgennmByAgenno'; //廠商代碼是否存在
    var AgennoComboGet = '/api/AA0038/GetAgennoCombo';       //廠商代碼
    var sourceGet = '/api/AA0125/Get';
    var T1Name = "次月衛材基本檔異動";
    var lType = 'N';

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var vtype = Ext.getUrlParam('strVtype');
    var mmcode = Ext.getUrlParam('strMmcode');
    var formMmcodeReadonly = true;
    var formReadonly = true;
    var formAllowblank = true;
    var formCls = 'required';
    var defaultBeginDate = '';
    var defaultEndDate = '';
    if (vtype == 'I' || vtype == 'U')
    {
        lType = 'N';
        formReadonly = false;
        formAllowblank = false;
        formCls = 'required';
        if (vtype == 'I')
        {
            formMmcodeReadonly = false;
            T1Set = '/api/AA0125/Create';    //MI_MAST_N
            sourceGet = '/api/AA0038/Get';
        }
        else if (vtype == 'U')
        {
            formMmcodeReadonly = true;
            T1Set = '/api/AA0125/Update';    //MI_MAST_N
        }
    }
    else if (vtype == 'V')
    {
        lType = 'N';
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
            params: { p0: 'M' },
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
    }
    setComboData();
    function getDefaultBeginDate() {
        Ext.Ajax.request({
            url: '/api/AA0125/getDefaultBeginDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.etts.length > 0) {
                        defaultBeginDate = data.etts[0].VALUE;
                        defaultEndDate = data.etts[0].TEXT;
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getDefaultBeginDate();

    var T1Store = Ext.create('WEBAPP.store.MiMastMaintain', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: sourceGet,
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: mmcode,
                    loadType: lType
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
               
                if (records.length > 0) {
                    var f = T1Form.getForm();

                    T1Form.loadRecord(records[0]);

                    // 若起訖日不為空放入DATE欄味
                    if (f.findField('BEGINDATE_DATE').getValue() != null) {
                        f.findField('BEGINDATE_DATE').setValue(new Date(f.findField('BEGINDATE_DATE').getValue()));
                    } else {
                        f.findField('BEGINDATE_DATE').reset();
                    }
                    if (f.findField('ENDDATE_DATE').getValue() != null) {
                        f.findField('ENDDATE_DATE').setValue(new Date(f.findField('ENDDATE_DATE').getValue()));
                    } else {
                        f.findField('ENDDATE_DATE').reset();
                    }
                    // 新增或修改
                    if (vtype == 'I' || vtype == 'U') {
                        var f = T1Form.getForm();
                        // 衛材一般物品共同項目
                        // 申請申購識別碼 M_APPLYID,
                        // 最小撥補量 MIN_ORDQTY,
                        // 申購計量單位 M_PURUN,
                        // 廠商包裝轉換率 EXCH_RATIO
                        f.findField('M_APPLYID').setReadOnly(false);
                        f.findField('MIN_ORDQTY').setReadOnly(false);
                        f.findField('M_PURUN').setReadOnly(false);
                        f.findField('EXCH_RATIO').setReadOnly(false);
                        // 一般物品項目
                        // 廠商代碼 M_AGENNO
                        // 合約單價 M_CONTPRICE
                        // 中文品名 MMNAME_C
                        // 庫備識別碼 M_STOREID
                        // 合約識別碼 M_CONTID
                        // 生效起日 BEGINDATE_DATE，儲存時取民國年月日存入BEGINDATE
                        // 生效迄日 ENDDATE_DATE，儲存時取民國年月日存入ENDDATE
                        // 環保或衛署許可證 M_PHCTNCO (2021-01-15新增修改需求)
                        // 環保證號效期 M_ENVDT (2021-01-15新增修改需求)
                        f.findField('M_CONTPRICE').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('MMNAME_C').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('M_STOREID').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('M_CONTID').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('BEGINDATE_DATE').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('ENDDATE_DATE').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('M_AGENNO').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('M_PHCTNCO').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        f.findField('M_ENVDT').setFieldStyle({ backgroundColor: 'lightgrey', backgroundImage: 'none' });
                        
                        if (f.findField('MAT_CLASS').getValue() != '02') {
                            f.findField('M_CONTPRICE').setReadOnly(false);
                            f.findField('MMNAME_C').setReadOnly(false);
                            f.findField('M_STOREID').setReadOnly(false);
                            f.findField('M_CONTID').setReadOnly(false);
                            f.findField('BEGINDATE_DATE').enable();
                            f.findField('ENDDATE_DATE').enable();
                            f.findField('M_AGENNO').setReadOnly(false);
                            f.findField('M_PHCTNCO').setReadOnly(false);
                            f.findField('M_ENVDT').setReadOnly(false);

                            f.findField('M_CONTPRICE').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('MMNAME_C').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('M_STOREID').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('M_CONTID').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('BEGINDATE_DATE').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('ENDDATE_DATE').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('M_AGENNO').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('M_PHCTNCO').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                            f.findField('M_ENVDT').setFieldStyle({ backgroundColor: '#fff', backgroundImage: 'none' });
                        }
                        
                        if (vtype == 'I' && f.findField('MAT_CLASS').getValue() != '02') {
                            f.findField('BEGINDATE_DATE').setValue(new Date(defaultBeginDate));
                            f.findField('ENDDATE_DATE').setValue(new Date(defaultEndDate));
                        }

                    } 

                    //if (records[0].data['MAT_CLSID'] == '3') {
                    //    // 若為一般物品,最小撥補量為必填
                    //    T1Form.getForm().findField('MIN_ORDQTY').setFieldStyle('{color:black; border:0; background-color:#ffc0cb; background-image:none;}');
                    //    T1Form.getForm().findField('MIN_ORDQTY').allowBlank = false;
                    //}
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
    var T1FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        limit: 20,
        queryUrl: MmcodeComboGet,
        //storeAutoLoad: true,
        colspan: 1,
        width: col_Wid,
        readOnly: formMmcodeReadonly,
        allowBlank: formAllowblank,
        fieldCls: formCls,
        matchFieldWidth: false,
        listConfig: { width: 250 },
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        getDefaultParams: function () {
            var f = T1Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                tmpArray = f.findField("MAT_CLASS").getValue().split(' ');
                return {
                    MAT_CLASS: tmpArray[0]
                };
            }
        },
        listeners: {
            select: function (field, record, eOpts) {
                var f = T1Form.getForm();
                if (record.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(record.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(record.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(record.get('MMNAME_E'));
                    mmcode = record.get('MMCODE');
                    T1Load();
                }
            }
        }
    });
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
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        readOnly: true
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
        readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 150 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        readOnly: true
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
        //readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        //readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        //readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        autoSelect: true,
        readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
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
        //readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        //readOnly: formReadonly,
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
    });
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'M_AGENNO',
        fieldLabel: '廠商代碼',
        limit: 20,
        queryUrl: AgennoComboGet,
        //disabled: true,
        //storeAutoLoad: true,
        readOnly: true,
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
        //readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 300 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
        //readOnly: formReadonly,
        matchFieldWidth: false,
        listConfig: { width: 160 },
        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
        fieldCls: 'readOnly',
        readOnly: true
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
                            //{
                            //    xtype: 'textfield',
                            //    fieldLabel: '院內碼',
                            //    name: 'MMCODE',
                            //    labelAlign: 'right',
                            //    enforceMaxLength: true,
                            //    maxLength: 13,
                            //    regexText: '只能輸入英文字母與數字',
                            //    regex: /^[\w]{0,13}$/,
                            //    allowBlank: formAllowblank,
                            //    fieldCls: formCls,
                            //    labelWidth: col_labelWid,
                            //    width: col_Wid * 2,
                            //    colspan: 4
                            //},
                            T1FormMmcode, {
                                xtype: 'combo',
                                store: matclassFormStore,
                                id: 'MAT_CLASS',
                                name: 'MAT_CLASS',
                                displayField: 'COMBITEM',
                                valueField: 'VALUE',
                                fieldLabel: '物料分類代碼',
                                queryMode: 'local',
                                //readOnly: formReadonly,
                                autoSelect: true,
                                multiSelect: false,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1,
                                anyMatch: true,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                //readOnly: formReadonly,
                                multiSelect: false,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, cbCancelnote, {
                                xtype: 'textfield',
                                fieldLabel: '中文品名',
                                name: 'MMNAME_C',
                                id:'MMNAME_C',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 250,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4,
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '英文品名',
                                name: 'MMNAME_E',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 250,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '院內簡稱',
                                name: 'EASYNAME',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 60,
                                labelWidth: col_labelWid,
                                width: col_Wid * 3,
                                colspan: 4,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, cbMpaykind, cbMpayid, cbMtrnid, cbMapplyid, cbWexpid, cbWlocid, cbMstoreid, cbMcontid, cbMconsumid, {
                                xtype: 'textfield',
                                fieldLabel: '最小撥補量',
                                name: 'MIN_ORDQTY',
                                colspan: 4,
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                readOnly: formReadonly
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '環保或衛署許可證',
                                name: 'M_PHCTNCO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 120,
                                labelWidth: col_labelWid + 20,
                                width: col_Wid * 2,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '健保碼',
                                name: 'M_NHIKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 12,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '健保給付價',
                                name: 'NHI_PRICE',
                                colspan: 3,
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '聯標項次/院辦案號',
                                name: 'E_ITEMARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid + 20,
                                width: col_Wid,
                                colspan: 1,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '合約年度',
                                name: 'E_YRARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '聯標組別',
                                name: 'E_CLFARMYNO',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 10,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                colspan: 4,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '廠商名稱',
                                name: 'AGEN_NAMEC',
                                labelAlign: 'right',
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 1,
                            }, T1FormAgenno, {
                                xtype: 'textfield',
                                fieldLabel: '廠牌',
                                name: 'M_AGENLAB',
                                enforceMaxLength: true,
                                maxLength: 30,
                                labelWidth: col_labelWid,
                                width: col_Wid,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                readOnly: true,
                                listeners: {
                                    change: function (self, newValue, oldValue) {
                                        //var exch_radio = T1Form.getForm().findField('EXCH_RADIO').getValue();
                                    }
                                }
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '優惠合約單價',
                                name: 'DISC_CPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '折讓比',
                                name: 'M_DISCPERC',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '最小單價',
                                name: 'UPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '優惠最小單價',
                                name: 'DISC_UPRICE',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '廠商包裝轉換率',
                                name: 'EXCH_RATIO',
                                labelAlign: 'right',
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
                                //readOnly: formReadonly,
                                multiSelect: false,
                                editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                colspan: 3,
                                anyMatch: true,
                                listeners: {
                                    select: function (c, r, i, e) {
                                        var f = T1Form.getForm();
                                        f.findField("M_PURUN").setValue(r.get('VALUE'));
                                    }
                                },
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '長度(CM)',
                                name: 'M_VOLL',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '寬度(CM)',
                                name: 'M_VOLW',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '高度(CM)',
                                name: 'M_VOLH',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '圓周',
                                name: 'M_VOLC',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '材積轉換率',
                                name: 'M_SWAP',
                                labelAlign: 'right',
                                maskRe: /[0-9.]/,
                                regexText: '只能輸入數字及小數點',
                                regex: /^(([1-9][0-9]+)|([0-9]))(\.[0-9]+)?$/,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '自費價',
                                name: 'HOSP_PRICE',
                                labelAlign: 'right',
                                colspan: 2
                            }
                            , {
                                xtype: 'datefield',
                                format: 'Xmd',
                                fieldLabel: '生效起日',
                                name: 'BEGINDATE_DATE',
                                labelAlign: 'right',
                                allowBlank: true,
                                disabled: true
                            }
                            //, {
                            //    xtype: 'textfield',
                            //    format:'Xmd',
                            //    fieldLabel: '生效起日',
                            //    name: 'BEGINDATE',
                            //    labelAlign: 'right',
                            //    readOnly: true
                            //}
                            , {
                                xtype: 'datefield',
                                format: 'Xmd',
                                fieldLabel: '生效迄日',
                                name: 'ENDDATE_DATE',
                                labelAlign: 'right',
                                allowBlank: true,
                                disabled: true
                            }
                            //, {
                            //    xtype: 'textfield',
                            //    format: 'Xmd',
                            //    fieldLabel: '生效迄日',
                            //    name: 'ENDDATE',
                            //    labelAlign: 'right',
                            //    readOnly: true
                            //}
                            , {
                                xtype: 'textfield',
                                fieldLabel: 'ID碼',
                                hidden: true,
                                name: 'M_IDKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 8,
                                fieldCls: 'readOnly',
                                readOnly: true
                            },
                            {
                                xtype: 'textfield',
                                fieldLabel: '衛材料號碼',
                                hidden: true,
                                name: 'M_INVKEY',
                                labelAlign: 'right',
                                enforceMaxLength: true,
                                maxLength: 16,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                colspan: 2,
                                fieldCls: 'readOnly',
                                readOnly: true
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
                                //readOnly: formReadonly,
                                multiSelect: false,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                fieldCls: 'readOnly',
                                readOnly: true
                            }, cbContracno, {
                                xtype: 'hidden',
                                fieldLabel: 'Update',
                                name: 'x',
                            }, {
                                xtype: 'hidden',
                                name: 'BEGINDATE',
                                submitValue: true,
                            }, {
                                xtype: 'hidden',
                                name: 'ENDDATE',
                                submitValue: true,
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
        f.findField('BEGINDATE').setValue(f.findField('BEGINDATE_DATE').rawValue);
        f.findField('ENDDATE').setValue(f.findField('ENDDATE_DATE').rawValue);
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
                            //設定TimeOut時間
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

    // 一般物品,能設,通信的優惠合約單價,最小數量,優惠最小單價不能直接維護,而由合約單價和廠商包裝轉換率計算
    // 2020.05.19增加衛材也套用此規則
    T1Form.getForm().findField('UPRICE').setReadOnly(true);
    T1Form.getForm().findField('DISC_CPRICE').setReadOnly(true);
    T1Form.getForm().findField('DISC_UPRICE').setReadOnly(true);

    if (vtype == 'I')
    {
        T1Form.getForm().findField('MAT_CLASS').setValue('02');
        T1Form.getForm().findField('M_APPLYID').setValue('Y');
        T1Form.getForm().findField('CANCEL_ID').setValue('N');
        T1Form.getForm().findField('WEXP_ID').setValue('Y');
        T1Form.getForm().findField('WLOC_ID').setValue('N');
    }
    else if (vtype == 'V' || vtype == 'U')
        T1Load();

    if (vtype == 'I' || vtype == 'U') {
        T1Form.down('#formsave').setVisible(true);
    }
    
});