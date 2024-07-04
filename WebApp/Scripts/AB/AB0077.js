﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AB0077/GetWH_NOComboOne';
    var DiffClsComboGet = '../../../api/AB0077/GetDiffClsComboOne';
    var MiMcodeComboGet = '../../../api/AB0077/GetMiMcodes';
    var T1GetExcel = '../../../api/AB0077/Excel';
    var reportUrl = '/Report/A/AB0077.aspx';

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    // 表單類別
    var st_DiffCls = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 合約碼
    var contractNoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { 'VALUE': '', 'TEXT': '全部' },
            { 'VALUE': 'A', 'TEXT': '合約品項' },
            { 'VALUE': 'B', 'TEXT': '非合約品項' },
            { 'VALUE': 'C', 'TEXT': '其他' }
        ]
    });

    // 庫存異動
    var mimcodeStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var defaultSorter = [{ property: 'MMCODE', direction: 'ASC' }];
    var ph1sSorter = [{ property: 'T_DATE', direction: 'DESC' }, { property: 'T_HM', direction: 'DESC' }];

    var T2QueryMMCodeStart = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P7',
        name: 'P7',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 174,
        labelWidth: 74,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0077/GetMMCODEComboOne', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                
                if (T1Query.getForm().findField('P0').getValue() == 'PH1S') {
                    T1Query.getForm().findField('P8').setValue(r.data.MMCODE);
                }
            }
        },
    });

    var T2QueryMMCodeEnd = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P8',
        fieldLabel: '至',
        labelSeparator: '',
        labelWidth: 16,
        width:116,
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0077/GetMMCODEComboOne', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    function setComboData() {

        Ext.Ajax.request({
            url: WH_NOComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;
                    var combo_P0 = T1Query.getForm().findField('P0');

                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            wh_noStore.add({ WH_NO: tb_wh_no[i].VALUE, WH_NAME: tb_wh_no[i].COMBITEM });
                            combo_P0.setValue('PH1S'); //預設為PH1S_藥庫
                        }
                    }

                    // 出入庫預設入庫
                    if (combo_P0.getValue() == 'PH1S') {
                        
                        T1Query.getForm().findField('P2').setValue({ P2: 'I' });
                        T1Query.getForm().findField('P3').setValue(new Date('2020-08-01'));

                    } else {
                        T1Query.getForm().findField('P2').setValue({ P2: 'IO' });
                        T1Query.getForm().findField('P3').setValue(getDefaultValue(true));
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: DiffClsComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_DiffCls = data.etts;
                    var combo_P1 = T1Query.getForm().findField('P1');

                    if (tb_DiffCls.length > 0) {
                        for (var i = 0; i < tb_DiffCls.length; i++) {
                            st_DiffCls.add({ VALUE: tb_DiffCls[i].VALUE, COMBITEM: tb_DiffCls[i].COMBITEM });
                            combo_P1.setValue(' '); //預設為全部
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: MiMcodeComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var mimcodes = data.etts;
                    
                    if (mimcodes.length > 0) {
                        mimcodeStore.add({ VALUE: '', TEXT:'全部'});
                        for (var i = 0; i < mimcodes.length; i++) {
                            mimcodeStore.add({ VALUE: mimcodes[i].VALUE, TEXT: mimcodes[i].TEXT });
                        }
                        T1Query.getForm().findField('P9').setValue(''); //預設為全部
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

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
                    xtype: 'combo',
                    store: wh_noStore,
                    fieldLabel: '庫別',
                    name: 'P0',
                    id: 'P0',
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
                            // 出入庫預設入庫
                            if (records.data.WH_NO == 'PH1S') {
                                
                                T1Query.getForm().findField('P2').setValue({ P2: 'I' });
                                T1Query.getForm().findField('P3').setValue(new Date('2020-08-01'));

                            } else {
                                T1Query.getForm().findField('P2').setValue({ P2: 'IO' });
                                T1Query.getForm().findField('P3').setValue(getDefaultValue(false));
                            }
                        }
                    },
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'combo',
                    store: st_DiffCls,
                    fieldLabel: '表單類別',
                    name: 'P1',
                    id: 'P1',
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    padding: '0 4 0 4',
                    //listeners: {
                    //    select: function (combo, records, eOpts) {
                    //        var P1_Value = T1Query.getForm().findField('P1').getRawValue();
                    //        if (P1_Value == '藥品進貨') {
                    //            Ext.getCmp('P5').setVisible(true);
                    //        }
                    //        else {
                    //            Ext.getCmp('P5').setVisible(false);
                    //        }
                    //    }
                    //}
                },
                {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    //fieldLabel: '報表',
                    labelWidth: 65,
                    width: 190,
                    name: 'P2',
                    id: 'P2',
                    items: [
                        {
                            boxLabel: '出入庫',
                            width: 70,
                            name: 'P2',
                            inputValue: 'IO',
                            checked: true
                        },
                        {
                            boxLabel: '出庫',
                            width: 60,
                            name: 'P2',
                            inputValue: 'O'
                        },
                        {
                            boxLabel: '入庫',
                            width: 60,
                            name: 'P2',
                            inputValue: 'I'
                        }
                    ],
                    padding: '0 4 0 15'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 45px;',
                    handler: function () {
                        if (Ext.getCmp('P0').validate() && Ext.getCmp('P1').validate()
                            && Ext.getCmp('P3').validate() && Ext.getCmp('P4').validate()) {
                            T1Load();
                        }
                        else {
                            Ext.Msg.alert('訊息', '庫別、表單類別、異動日期起迄為必填條件');
                        }
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
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '異動日期',
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 74,
                    width: 164,
                    value: getDefaultValue(false),
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    id: 'P4',
                    labelWidth: 20,
                    width: 110,
                    value: getDefaultValue(true),
                    fieldCls: 'required',
                    allowBlank: false,
                },
                {
                    xtype: 'radiogroup',
                    anchor: '40%',
                    name: 'P5',
                    id: 'P5',
                    items: [
                        {
                            boxLabel: '合約、非合約',
                            width: 90,
                            name: 'P5',
                            checked: true,
                            inputValue: 'c',
                        },
                        {
                            boxLabel: '優惠百分比',
                            width: 90,
                            name: 'P5',
                            inputValue: 'p',
                        }
                    ],
                    padding: '0 0 0 20'
                },
                {
                    xtype: 'combo',
                    store: contractNoStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    fieldLabel: '合約碼',
                    name: 'P6',
                    id: 'P6',
                    enforceMaxLength: true,
                    padding: '0 0 0 10',
                    width: 180

                },
                {
                    xtype: 'combo',
                    store: mimcodeStore,
                    fieldLabel: '庫存異動',
                    name: 'P9',
                    id: 'P9',
                    queryMode: 'local',
                    allowBlank: true,
                    anyMatch: true,
                    autoSelect: true,
                    width: 200,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    padding: '0 4 0 4'
                },
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                T2QueryMMCodeStart,
                T2QueryMMCodeEnd,
                 {
                xtype: 'textfield',
                fieldLabel: '表單號碼',
                name: 'P10',
                id: 'P10',
                padding: '0 4 0 4',
            },
            ]
        }]
    });

    function getDefaultValue(isEndDate) {
        tmp_Date = new Date();
        if (!isEndDate) {
            tmp_Date.setDate(tmp_Date.getDate() - 3);//3天前的日期
            var y = tmp_Date.getFullYear() - 1911;
            var m = tmp_Date.getMonth() + 1;
            var d = tmp_Date.getDate();
            m = m > 9 ? m.toString() : "0" + m.toString();
            d = d > 9 ? d.toString() : "0" + d.toString();
            tmp_Date = y + m + d;
        }
        return tmp_Date;
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'TR_DOCNO', type: 'string' },
            { name: 'WH_NO', type: 'string' },
            { name: 'T_DATE', type: 'string' },
            { name: 'T_HM', type: 'string' },
            { name: 'TR_DOCTYPE', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'TR_IO', type: 'string' },
            { name: 'TR_INV_QTY', type: 'string' },
            { name: 'T_MONEY', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'T_NOTES', type: 'string' },
            { name: 'T_APPQTY', type: 'string' },
            { name: 'T_LOT_NO', type: 'string' },
            { name: 'T_EXPDATE', type: 'string' },
            { name: 'T_OP', type: 'string' },
            { name: 'LOTNO_EXP_QTY', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'T_DATE', direction: 'DESC' }, { property: 'T_HM', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            timeout: 0,
            url: '/api/AB0077/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件值代入參數

                
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue().P2,
                    p3: T1Query.getForm().findField('P3').getRawValue(),
                    p4: T1Query.getForm().findField('P4').getRawValue(),
                    p5: T1Query.getForm().findField('P5').getValue().P5,
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: T1Query.getForm().findField('P9').getValue(),
                    p10: T1Query.getForm().findField('P10').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export',
                text: '匯出',
                handler: function () {
                    if (Ext.getCmp('P0').validate() && Ext.getCmp('P1').validate()
                        && Ext.getCmp('P3').validate() && Ext.getCmp('P4').validate()) {
                        var today = getTodayDate();
                        var p = new Array();
                        p.push({ name: 'FN', value: today + '_單位品項基準量.xls' });
                        p.push({ name: 'P0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'P1', value: T1Query.getForm().findField('P1').getValue() });
                        p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue().P2 });
                        p.push({ name: 'P3', value: T1Query.getForm().findField('P3').getRawValue() });
                        p.push({ name: 'P4', value: T1Query.getForm().findField('P4').getRawValue() });
                        p.push({ name: 'P5', value: T1Query.getForm().findField('P5').getValue().P5 });
                        p.push({ name: 'P6', value: T1Query.getForm().findField('P6').getValue() });
                        p.push({ name: 'P7', value: T1Query.getForm().findField('P7').getValue() });
                        p.push({ name: 'P8', value: T1Query.getForm().findField('P8').getValue() });
                        PostForm(T1GetExcel, p);
                    } else {
                        Ext.Msg.alert('訊息', '需填庫別、表單類別、異動日期起迄才能匯出');
                    }
                }
            },
            {
                itemId: 'print',
                text: '列印',
                handler: function () {
                    if (Ext.getCmp('P0').validate() && Ext.getCmp('P1').validate()
                        && Ext.getCmp('P3').validate() && Ext.getCmp('P4').validate()) {
                        showReport();
                    } else {
                        Ext.Msg.alert('訊息', '需填庫別、表單類別、異動日期起迄才能列印');
                    }
                }
            }
        ]
    });

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue().P2,
                p3: T1Query.getForm().findField('P3').getRawValue(),
                p4: T1Query.getForm().findField('P4').getRawValue(),
                p5: T1Query.getForm().findField('P5').getValue().P5,
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue(),
                p8: T1Query.getForm().findField('P8').getValue(),
                p9: T1Query.getForm().findField('P9').getValue(),
                p10: T1Query.getForm().findField('P10').getValue(),
            };

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                        + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3
                        + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '&p7=' + np.p7
                        + '&p8=' + np.p8 + '&p9=' + np.p9 + '&p10=' + np.p10
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

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

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
            items: [T1Query]
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "異動日期",
            dataIndex: 'T_DATE',
            width: 100
        }, {
            text: "異動時間",
            dataIndex: 'T_HM',
            width: 80
        }, {
            text: "類別",
            dataIndex: 'TR_DOCTYPE',
            width: 90
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "藥品名稱",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 60
        }, {
            text: "進出數量",
            dataIndex: 'TR_INV_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "金額",
            dataIndex: 'T_MONEY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "入庫別",
            dataIndex: 'TOWH',
            width: 60
        }, {
            text: "出庫別",
            dataIndex: 'FRWH',
            width: 60
        }, {
            text: "備註",
            dataIndex: 'T_NOTES',
            width: 130
        },
        {
            text: "操作者",
            dataIndex: 'T_OP',
            width: 100
        },
        {
                text: "批號 - 效期 - 數量",
                dataIndex: 'LOTNO_EXP_QTY',
                width: 300
        }, {
            text: "表單號碼",
            dataIndex: 'TR_DOCNO',
            width: 120
        }, {
            text: "庫存異動",
            dataIndex: 'TR_MCODE',
            width: 120
        },{
            header: "",
            flex: 1
        }],
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
            items: [T1Grid]
        },
        ]
    });

    //Radiogroup預設為日期
    //Ext.getCmp('P1_Date').show();
    //Ext.getCmp('P1_Month').hide();
    Ext.getCmp('P5').hide();
    T1Query.getForm().findField('P6').setValue('');
});