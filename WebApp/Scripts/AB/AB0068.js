﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var reportUrl = '/Report/A/AB0068.aspx';
    var T1GetExcel = '/api/AB0068/Excel';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AB0068/GetWH_NOComboOne';
    var radioSelected = 'isCombo';

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    
    var isWard = Ext.getUrlParam('isward');

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    // 藥品分類
    var st_MedicineClass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { TEXT: '一~三級管制用藥', VALUE: '1' },
            { TEXT: '四級管制用藥', VALUE: '4' },
            { TEXT: '其他列管藥品', VALUE: '0' },
            { TEXT: '非管制用藥', VALUE: 'N' }
        ]
    });

    // 高價藥、 CDC、全院停用碼
    var drugYNTypeClass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { 'VALUE': '', 'TEXT': '全部' },
            { 'VALUE': 'Y', 'TEXT': '是' },
            { 'VALUE': 'N', 'TEXT': '否' }
        ]
    });
    // 各庫停用碼 0.繼續使用, 1.刪除, 2.停產, 3.廠缺, 4.斷貨
    var ctdmdccodeClass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { 'VALUE': '', 'TEXT': '全部' },
            { 'VALUE': '0', 'TEXT': '繼續使用' },
            { 'VALUE': '1', 'TEXT': '刪除' },
            { 'VALUE': '2', 'TEXT': '停產' },
            { 'VALUE': '3', 'TEXT': '廠缺' },
            { 'VALUE': '4', 'TEXT': '斷貨' },
        ]
    });

    function setComboData(isWard) {
        Ext.Ajax.request({
            url: WH_NOComboGet,
            params: { limit: 10, page: 1, start: 0, isWard: isWard },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P5 = T1Query.getForm().findField('P5');
                    var combo_P5_ward = T1Query.getForm().findField('P5_ward');

                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            wh_noStore.add({ WH_NO: tb_wh_no[i].VALUE, WH_NAME: tb_wh_no[i].COMBITEM });
                            combo_P5.setValue('CHEMO'); //預設為CHEMO_內湖化療調配室
                        }
                    }
                    combo_P5_ward.setValue(tb_wh_no[0].VALUE);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData(isWard);

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    fieldLabel: '報表',
                    labelWidth: 65,
                    width: 210,
                    name: 'P0',
                    id: 'P0',
                    items: [
                        {
                            boxLabel: '日報',
                            width: 60,
                            name: 'P0',
                            inputValue: 1,
                            checked: true,
                            handler: function () {
                                Ext.getCmp('P1_Date').setVisible(false);
                                Ext.getCmp('P1_Month').setVisible(true);
                            }
                        },
                        {
                            boxLabel: '月報',
                            width: 60,
                            name: 'P0',
                            inputValue: 2,
                            handler: function () {
                                Ext.getCmp('P1_Date').setVisible(true);
                                Ext.getCmp('P1_Month').setVisible(false);
                            }
                        }
                    ],
                    padding: '0 4 0 0'
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '日期',
                    name: 'P1_Date',
                    id: 'P1_Date',
                    width: 130,
                    labelWidth:50, 
                    fieldCls: 'required',
                    allowBlank: false,
                    minValue: new Date(2020, 7, 1),
                    maxValue: new Date()
                },
                {
                    xtype: 'monthfield',
                    fieldLabel: '日期',
                    name: 'P1_Month',
                    id: 'P1_Month',
                    width: 150,
                    fieldCls: 'required',
                    allowBlank: false,
                },
                {
                    xtype: 'combo',
                    store: drugYNTypeClass,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '高價藥品',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150

                },
                {
                    xtype: 'combo',
                    store: drugYNTypeClass,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: 'CDC用藥',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150

                },
                {
                    xtype: 'combo',
                    store: drugYNTypeClass,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '全院停用碼',
                    name: 'P6',
                    id: 'P6',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150

                },
                {
                    xtype: 'combo',
                    store: ctdmdccodeClass,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '各庫停用碼',
                    name: 'P7',
                    id: 'P7',
                    enforceMaxLength: true,
                    padding: '0 4 0 4',
                    width: 150

                },
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '藥品分類',
                    store: st_MedicineClass,
                    name: 'P4',
                    id: 'P4',
                    labelWidth: 80,
                    width: 220,
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    displayField: 'TEXT',
                    valueField: 'VALUE'
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫別',
                    labelWidth: 40,
                    width: 45,
                },
                {
                    xtype: 'combo',
                    store: wh_noStore,
                    //fieldLabel: '庫別',
                    name: 'P5_ward',
                    id: 'P5_ward',
                    width: 200,
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    hidden: true,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 0'
                },
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P5_radio',
                    id: 'radio1',
                    width: 15,
                    checked: true,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isCombo';
                                T1Query.getForm().findField('P5').enable();
                            }
                        }
                    }
                },
                {
                    xtype: 'combo',
                    store: wh_noStore,
                    //fieldLabel: '庫別',
                    name: 'P5',
                    id: 'P5',
                    width: 200,
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 0'
                },
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P5_radio',
                    id: 'radio2',
                    width: 15,
                    checked: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'isPhd';
                                T1Query.getForm().findField('P5').disable();
                            }
                        }
                    }
                },
                {
                    xtype: 'displayfield',
                    value: '全藥局',
                    id: 'allPhd',
                    width: 60
                },
                {
                    xtype: 'radiofield',
                    boxLabel: '',
                    name: 'P5_radio',
                    id: 'radio3',
                    width: 15,
                    checked: false,
                    listeners: {
                        change: function (self, newValue, oldValue, eOpts) {
                            if (newValue == true) {
                                radioSelected = 'all';
                                T1Query.getForm().findField('P5').disable();
                            }
                        }
                    }
                },
                {
                    xtype: 'displayfield',
                    value: '全院',
                    id: 'all',
                    width: 60
                },
                
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 45px;',
                    handler: function () {
                        //showReport(true);
                        
                        if (isWard == 'Y') {
                            if (!T1Query.getForm().findField('P5_ward').getValue() ||
                                (T1Query.getForm().findField('P4').getValue() == '') ||
                                (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                                (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                                Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                                return;
                            }
                        } else {
                            if ((radioSelected == 'isCombo' && T1Query.getForm().findField('P5').getValue() == '') ||
                                (T1Query.getForm().findField('P4').getValue() == '') ||
                                (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                                (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                                Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                                return;
                            }
                        }
                        
                        if (T1Query.getForm().findField('P0').getValue().P0 == '1') {
                            if (Number(T1Query.getForm().findField('P1_Date').rawValue) < 1090801) {
                                Ext.Msg.alert('訊息', '<span style="color:red">日期</span>不可早於1090801');
                                return;
                            }
                        }

                        if (T1Query.getForm().findField('P0').getValue().P0 == '2') {
                            if (Number(T1Query.getForm().findField('P1_Month').rawValue) < 10908) {
                                Ext.Msg.alert('訊息', '<span style="color:red">月份</span>不可早於10908');
                                return;
                            }
                        }

                        T1Load();
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
            },
        ]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'WH_NO', type: 'string' },
            { name: 'WH_NAME', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BF_QTY', type: 'string' },
            { name: 'IN_SUM', type: 'string' },
            { name: 'OUT_SUM', type: 'string' },
            { name: 'USEO_QTY', type: 'string' },
            { name: 'RS_QTY', type: 'string' },
            { name: 'CHIO_QTY', type: 'string' },
            { name: 'ADJ_QTY', type: 'string' },
            { name: 'AF_QTY', type: 'string' },
            { name: 'LOSS_QTY', type: 'string' },
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'WH_NO', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            timeout: 1800000,
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0068/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                if (isWard) {
                    np = {
                        p0: T1Query.getForm().findField('P0').getValue().P0,
                        p1: T1Query.getForm().findField('P0').getValue().P0 == '1' ?
                            T1Query.getForm().findField('P1_Date').getRawValue() : T1Query.getForm().findField('P1_Month').getRawValue(),
                        p2: T1Query.getForm().findField('P2').getValue(),    //getValue()後是RadioGroup物件，需要再選擇該物件的屬性名稱
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: T1Query.getForm().findField('P4').getValue(),
                        p4_Name: T1Query.getForm().findField('P4').getRawValue(),
                        p5: T1Query.getForm().findField('P5_ward').getValue(),
                        p5_Name: T1Query.getForm().findField('P5_ward').getRawValue(),
                        p6: T1Query.getForm().findField('P6').getValue(),
                        p7: T1Query.getForm().findField('P7').getValue(),
                        isWard: 'Y'
                    };
                } else {
                    // 載入前將查詢條件P0值代入參數
                    np = {
                        p0: T1Query.getForm().findField('P0').getValue().P0,
                        p1: T1Query.getForm().findField('P0').getValue().P0 == '1' ?
                            T1Query.getForm().findField('P1_Date').getRawValue() : T1Query.getForm().findField('P1_Month').getRawValue(),
                        p2: T1Query.getForm().findField('P2').getValue(),    //getValue()後是RadioGroup物件，需要再選擇該物件的屬性名稱
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: T1Query.getForm().findField('P4').getValue(),
                        p4_Name: T1Query.getForm().findField('P4').getRawValue(),
                        p5: radioSelected == 'isCombo' ? T1Query.getForm().findField('P5').getValue() :
                            (radioSelected == 'isPhd' ? 'isPhd' : 'all'),
                        p5_Name: T1Query.getForm().findField('P5').getRawValue(),
                        p6: T1Query.getForm().findField('P6').getValue(),
                        p7: T1Query.getForm().findField('P7').getValue(),
                        isWard: 'N'
                    };
                }
                
                Ext.apply(store.proxy.extraParams, np);
            }
        },
          
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出',
                handler: function () {

                    if (isWard == 'Y') {
                        if (!T1Query.getForm().findField('P5_ward').getValue() ||
                            (T1Query.getForm().findField('P4').getValue() == '') ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                            return;
                        }
                    } else {
                        if ((radioSelected == 'isCombo' && T1Query.getForm().findField('P5').getValue() == '') ||
                            (T1Query.getForm().findField('P4').getValue() == '') ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                            return;
                        }
                    }

                    if (T1Query.getForm().findField('P0').getValue().P0 == '1') {
                        if (Number(T1Query.getForm().findField('P1_Date').rawValue) < 1090801) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>不可早於1090801');
                            return;
                        }
                    }

                    if (T1Query.getForm().findField('P0').getValue().P0 == '2') {
                        if (Number(T1Query.getForm().findField('P1_Month').rawValue) < 10908) {
                            Ext.Msg.alert('訊息', '<span style="color:red">月份</span>不可早於10908');
                            return;
                        }
                    }

                    var p = new Array();

                    var dateValue = T1Query.getForm().findField('P0').getValue().P0 == '1' ?
                        T1Query.getForm().findField('P1_Date').getRawValue() : T1Query.getForm().findField('P1_Month').getRawValue();

                    var p5_value;
                    var p5_name_value;
                    if (isWard) {
                        p5_value = T1Query.getForm().findField('P5_ward').getValue();
                        p5_name_value = T1Query.getForm().findField('P5_ward').getRawValue();
                    } else {
                        p5_value = radioSelected == 'isCombo' ? T1Query.getForm().findField('P5').getValue() :
                            (radioSelected == 'isPhd' ? 'isPhd' : 'all')
                        p5_name_value = radioSelected == 'isCombo' ? T1Query.getForm().findField('P5').getRawValue() :
                            (radioSelected == 'isPhd' ? '全藥局' : '全院')
                    }
                
                    var file_name = '管制藥品及列管藥品出入帳明細報表_' + T1Query.getForm().findField('P4').getRawValue() + '_' + dateValue + '_' + p5_name_value  + '.xls';
                    p.push({ name: 'FN', value: file_name });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue().P0 });
                    p.push({
                        name: 'p1', value: dateValue,
                     });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p4_Name', value: T1Query.getForm().findField('P4').getRawValue() });
                    p.push({ name: 'p5', value: p5_value});
                    p.push({ name: 'p5_Name', value: p5_name_value });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });
                    p.push({ name: 'p7', value: T1Query.getForm().findField('P7').getValue() });
                    p.push({ name: 'isWard', value: isWard });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');

                }
            }, {
                text: '列印', handler: function () {

                    if (isWard == 'Y') {
                        if (!T1Query.getForm().findField('P5_ward').getValue() ||
                            (T1Query.getForm().findField('P4').getValue() == '') ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                            return;
                        }
                    } else {
                        if ((radioSelected == 'isCombo' && T1Query.getForm().findField('P5').getValue() == '') ||
                            (T1Query.getForm().findField('P4').getValue() == '') ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                            (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                            return;
                        }
                    }

                    if (T1Query.getForm().findField('P0').getValue().P0 == '1') {
                        if (Number(T1Query.getForm().findField('P1_Date').rawValue) < 1090801) {
                            Ext.Msg.alert('訊息', '<span style="color:red">日期</span>不可早於1090801');
                            return;
                        }
                    }

                    if (T1Query.getForm().findField('P0').getValue().P0 == '2') {
                        if (Number(T1Query.getForm().findField('P1_Month').rawValue) < 10908) {
                            Ext.Msg.alert('訊息', '<span style="color:red">月份</span>不可早於10908');
                            return;
                        }
                    }

                    if (T1Store.getCount() > 0) {
                        showReport(true);
                    }
                    else {
                        Ext.Msg.alert('訊息', '無資料可列');
                    }
                }
            }
        ]
    });
    function T1Load() {
        T1Tool.moveFirst();
    }
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
            layout: 'fit',
            items: [T1Tool]    
        }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 90
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 90
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 90
            }, {
                text: "品項名稱",
                dataIndex: 'MMNAME_E',
                width: 200
            }, {
                text: "前日結存",
                dataIndex: 'BF_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "入帳",
                dataIndex: 'IN_SUM',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "出帳",
                dataIndex: 'OUT_SUM',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "醫令消耗",
                dataIndex: 'USEO_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "醫令退藥",
                dataIndex: 'RS_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "盤點差",
                dataIndex: 'CHIO_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "調帳",
                dataIndex: 'ADJ_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "耗損",
                dataIndex: 'LOSS_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            }, {
                text: "結存",
                dataIndex: 'AF_QTY',
                width: 90,
                align: 'right',
                style: 'text-align:left',
            },
        ]
    });

    function showReport(autoGen) {
        if (isWard == 'Y') {
            if (!T1Query.getForm().findField('P5_ward').getValue() ||
                (T1Query.getForm().findField('P4').getValue() == '') ||
                (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                return;
            }
        } else {
            if ((radioSelected == 'isCombo' && T1Query.getForm().findField('P5').getValue() == '') ||
                (T1Query.getForm().findField('P4').getValue() == '') ||
                (T1Query.getForm().findField('P0').getValue().P0 == '1' && T1Query.getForm().findField('P1_Date').validate() == false) ||
                (T1Query.getForm().findField('P0').getValue().P0 == '2' && T1Query.getForm().findField('P1_Month').validate() == false)) {
                Ext.Msg.alert('訊息', '<span style="color:red">日期</span>、<span style="color:red">藥品分類</span>及<span style="color:red">庫別</span>為必填');
                return;
            }
        }

        if (!win) {
            var np;
            
            if (isWard) {
                np = {
                    p0: T1Query.getForm().findField('P0').getValue().P0,
                    p1: T1Query.getForm().findField('P0').getValue().P0 == '1' ?
                        T1Query.getForm().findField('P1_Date').getRawValue() : T1Query.getForm().findField('P1_Month').getRawValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),    //getValue()後是RadioGroup物件，需要再選擇該物件的屬性名稱
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p4_Name: T1Query.getForm().findField('P4').getRawValue(),
                    p5: T1Query.getForm().findField('P5_ward').getValue(),
                    p5_Name: T1Query.getForm().findField('P5_ward').getRawValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    isWard:'Y'
                };
            } else {
                // 載入前將查詢條件P0值代入參數
                np = {
                    p0: T1Query.getForm().findField('P0').getValue().P0,
                    p1: T1Query.getForm().findField('P0').getValue().P0 == '1' ?
                        T1Query.getForm().findField('P1_Date').getRawValue() : T1Query.getForm().findField('P1_Month').getRawValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),    //getValue()後是RadioGroup物件，需要再選擇該物件的屬性名稱
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p4_Name: T1Query.getForm().findField('P4').getRawValue(),
                    p5: radioSelected == 'isCombo' ? T1Query.getForm().findField('P5').getValue() :
                        (radioSelected == 'isPhd' ? 'isPhd' : 'all'),
                    p5_Name: T1Query.getForm().findField('P5').getRawValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    isWard: 'N'
                };
            }

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2
                + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p4_Name=' + np.p4_Name + '&p5=' + np.p5 + '&p5_Name=' + np.p5_Name
                + '&p6=' + np.p6 + '&p7=' + np.p7 + '&isWard=' + np.isWard
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
                items: [T1Grid]
            },
        ]
    });

    //預設藥品分類為1
    T1Query.getForm().findField('P2').setValue('');
    T1Query.getForm().findField('P3').setValue('');
    T1Query.getForm().findField('P4').setValue('1');
    T1Query.getForm().findField('P6').setValue('N');
    T1Query.getForm().findField('P7').setValue('0');
    //Radiogroup預設為日期
    Ext.getCmp('P1_Date').show();
    Ext.getCmp('P1_Month').hide();
    if (isWard) {
        Ext.getCmp('P5_ward').show();

        Ext.getCmp('radio1').hide();
        Ext.getCmp('P5').hide();
        Ext.getCmp('radio2').hide();
        Ext.getCmp('allPhd').hide();
        Ext.getCmp('radio3').hide();
        Ext.getCmp('all').hide();
    } else {
        Ext.getCmp('P5_ward').hide();

        Ext.getCmp('radio1').show();
        Ext.getCmp('P5').show();
        Ext.getCmp('radio2').show();
        Ext.getCmp('allPhd').show();
        Ext.getCmp('radio3').show();
        Ext.getCmp('all').show();
    }

});