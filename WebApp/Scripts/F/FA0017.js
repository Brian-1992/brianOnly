﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    //var T1Set = ''; // 新增/修改/刪除
    var T1Name2 = "衛材採購月結報表";
    var reportUrl = '/Report/F/FA0017.aspx';
    var reportUrl2 = '/Report/F/FA0017_1.aspx';
    //var T1RecLength = 0;
    //var T1LastRec = null;
    //var T2RecLength = 0;
    //var T2LastRec = null;

    var T1GetExcel = '../../../api/FA0017/Excel';
    var T1GetTxt = '../../../api/FA0017/GetTxt';
    var T2GetExcel = '../../../api/FA0017/Excel2';
    var T2GetTxt = '../../../api/FA0017/GetTxt2';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    // 合約種類
    var st_Contract = Ext.create('Ext.data.Store', {
        fields: ['TEXT', 'VALUE']
    });

    // 是否合庫
    var st_IsTCB = Ext.create('Ext.data.Store', {
        fields: ['TEXT', 'VALUE']
    });

    function setComboData() {
        st_Contract.add(
            { TEXT: '合約', VALUE: 0 },
            { TEXT: '非合約', VALUE: 2 },
            { TEXT: '小採', VALUE: 3 }
        );

        st_IsTCB.add(
            { TEXT: '合庫', VALUE: 0 },
            { TEXT: '非合庫', VALUE: 1 },
            { TEXT: '不區分', VALUE: 2 }
        );
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'hbox',
            align: 'stretch',
            padding: 5
        },
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        name: 'P0',
                        id: 'P0',
                        labelWidth: 80,
                        width: 220,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        store: Ext.create('Ext.data.Store', {
                            fields: ['COMBITEM', 'VALUE'],
                            data: [
                                { COMBITEM: '衛材', VALUE: '02' }
                            ]
                        }),
                        value: '02'
                    }, {
                        xtype: 'monthfield',
                        fieldLabel: '月結年月',
                        name: 'P1',
                        id: 'P1',
                        fieldCls: 'required',
                        allowBlank: false,
                        labelWidth: 80,
                        width: 170,
                        value: new Date()
                    }, {
                        xtype: 'combo',
                        fieldLabel: '合約種類',
                        store: st_Contract,
                        name: 'P2',
                        id: 'P2',
                        labelWidth: 80,
                        fieldCls: 'required',
                        allowBlank: false,
                        width: 170,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        value: '0'
                    }, {
                        xtype: 'combo',
                        fieldLabel: '合庫否',
                        store: st_IsTCB,
                        name: 'P3',
                        id: 'P3',
                        labelWidth: 65,
                        width: 170,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        value:'2'
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
                        xtype: 'button',
                        text: '查詢',
                        style: 'margin:0px 5px 0px 20px;',
                        handler: function () {
                            if (!Ext.getCmp('P0').validate() || !Ext.getCmp('P1').validate() || !Ext.getCmp('P2').validate()) {

                                Ext.Msg.alert('訊息', '需填[物料分類]、[月結年月]及[合約種類]才能查詢');
                            }
                            else {
                                T1Load();
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
            }]
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: {
            type: 'hbox',
            align: 'stretch',
            padding: 5
        },
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'panel',
                id: 't2PanelP1',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '物料分類',
                        name: 't2P0',
                        id: 't2P0',
                        labelWidth: 80,
                        width: 260,
                        queryMode: 'local',
                        fieldCls: 'required',
                        allowBlank: false,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        store: Ext.create('Ext.data.Store', {
                            fields: ['COMBITEM', 'VALUE'],
                            data: [
                                { COMBITEM: '一般物品(03,04,05,06,08)', VALUE: '03' },
                                { COMBITEM: '被服', VALUE: '07' }
                            ]
                        }),
                        value: '03',
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '進貨日期',
                        name: 't2P1',
                        id: 't2P1',
                        labelWidth: mLabelWidth,
                        width: 150,
                        padding: '0 4 0 4',
                        fieldCls: 'required',
                        value: Ext.util.Format.date(new Date(), "Y-m-") + "01"
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 't2P2',
                        id: 't2P2',
                        labelWidth: 8,
                        width: 88,
                        padding: '0 4 0 4',
                        labelSeparator: '',
                        fieldCls: 'required',
                        value: new Date()
                    }, {
                        xtype: 'radiogroup',
                        fieldLabel: '中信或院內',
                        name: 't2P3',
                        id: 't2P3',
                        width: 270,
                        items: [
                            { boxLabel: '中信平台', width: 85, name: 't2P3', inputValue: 'Y' , checked: true },
                            { boxLabel: '院內購案', width: 85, name: 't2P3', inputValue: 'N' }
                        ]  
                    }
                ]
            },
            {
                xtype: 'panel',
                id: 't2PanelP2',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [
                    {
                        xtype: 'button',
                        text: '查詢',
                        style: 'margin:0px 5px 0px 20px;',
                        handler: function () {
                            if (!Ext.getCmp('t2P0').validate() || !Ext.getCmp('t2P1').validate() || !Ext.getCmp('t2P2').validate()) {
                                Ext.Msg.alert('訊息', '需填[物料分類]、[進貨日期]才能查詢');
                            }
                            else {
                                T2Load();
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
                            f.findField('t2P0').focus();
                            msglabel('訊息區:');
                        }
                    }
                ]
            }]
    });
    Ext.define('FA0017_Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'YYYMM', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'M_DISCPERC', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'AGEN_ACC', type: 'string' },
            { name: 'UNI_NO', type: 'string' },
            { name: 'FULLSUM', type: 'string' },
            { name: 'PAYSUM', type: 'string' },
            { name: 'DISCSUM', type: 'string' },
            { name: 'TXFEE', type: 'string' },
            { name: 'AGEN_BANK', type: 'string' },
            { name: 'AGEN_SUB', type: 'string' }
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'FA0017_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'agen_no', direction: 'ASC' }, { property: 'm_contid', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0017/All',
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
                    p1: T1Query.getForm().findField('P1').getRawValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('export').setDisabled(false);
                    Ext.getCmp('print').setDisabled(false);
                    if (T1Query.getForm().findField('P3').getValue() == '0' ||
                        T1Query.getForm().findField('P3').getValue() == '1') {
                        Ext.getCmp('exportTXT').setDisabled(false);
                    }
                } else {
                    Ext.getCmp('export').setDisabled(true);
                    Ext.getCmp('print').setDisabled(true);
                    Ext.getCmp('exportTXT').setDisabled(true);
                }
            }
        }
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'FA0017_Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'agen_no', direction: 'ASC' }, { property: 'm_contid', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/FA0017/All_2',
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
                    p0: T2Query.getForm().findField('t2P0').getValue(),
                    p1: T2Query.getForm().findField('t2P1').getRawValue(),
                    p2: T2Query.getForm().findField('t2P2').getRawValue(),
                    p3: T2Query.getForm().findField('t2P3').getValue()['t2P3']
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, options) {   //設定匯入,列印是否disable
                var dataCount = store.getCount();
                if (dataCount > 0) {
                    Ext.getCmp('t2export').setDisabled(false);
                    Ext.getCmp('t2print').setDisabled(false);
                } else {
                    Ext.getCmp('t2export').setDisabled(true);
                    Ext.getCmp('t2print').setDisabled(true);
                }
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        msglabel('訊息區:');
    }
    function T2Load() {
        T2Tool.moveFirst();
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
                itemId: 'export',
                id: 'export',
                name: 'export',
                text: '匯出',
                handler: function () {
                    if (!Ext.getCmp('P0').validate() || !Ext.getCmp('P1').validate() || !Ext.getCmp('P2').validate()) {

                        Ext.Msg.alert('訊息', '需填[物料分類]、[月結年月]及[合約種類]才能匯出');
                    } else {
                        var p0 = T1Query.getForm().findField('P0').getValue();
                        //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
                        var p0_Name = T1Query.getForm().findField('P0').getRawValue();
                        //p0_Name = Ext.util.Format.substr(p0_Name, 3, p0_Name.length);
                        var p1 = T1Query.getForm().findField('P1').getRawValue();
                        var p2 = T1Query.getForm().findField('P2').getValue();
                        var p2raw = T1Query.getForm().findField('P2').getRawValue();
                        var p3 = T1Query.getForm().findField('P3').getValue();
                        var p3raw = T1Query.getForm().findField('P3').getRawValue();
                        var p = new Array();
                        var filename = '中央庫房' + p1.substring(0, 3) + '年' + p1.substring(3, 5) + '月' + p0_Name + '類採購月結報表(' +p2raw+'('+p3raw+')).xls';
                        p.push({ name: 'FN', value: filename });
                        p.push({ name: 'P0', value: p0 });
                        p.push({ name: 'P1', value: p1 });
                        p.push({ name: 'P2', value: p2 });
                        p.push({ name: 'P3', value: p3 });
                        PostForm(T1GetExcel, p);
                    }
                }
            },
            {
                text: '列印',
                id: 'print',
                name: 'print',
                style: 'margin:0px 5px;',
                handler: function () {
                    if (!Ext.getCmp('P0').validate() || !Ext.getCmp('P1').validate() || !Ext.getCmp('P2').validate()) {

                        Ext.Msg.alert('訊息', '需填[物料分類]、[月結年月]及[合約種類]才能列印');
                    }
                    else {
                        showReport();
                    }
                }
            },
            {
                text: '匯出TXT',
                id: 'exportTXT',
                name: 'exportTXT',
                style: 'margin:0px 5px;',
                handler: function () {
                    if (!Ext.getCmp('P0').validate() || !Ext.getCmp('P1').validate() || !Ext.getCmp('P2').validate()) {

                        Ext.Msg.alert('訊息', '需填[物料分類]、[月結年月]及[合約種類]才能匯出TXT');
                    } else {
                        var p0 = T1Query.getForm().findField('P0').getValue();
                        //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
                        var p0_Name = T1Query.getForm().findField('P0').getRawValue();
                        //p0_Name = Ext.util.Format.substr(p0_Name, 3, p0_Name.length);
                        var p1 = T1Query.getForm().findField('P1').getRawValue();
                        var p2 = T1Query.getForm().findField('P2').getValue();
                        var p3 = T1Query.getForm().findField('P3').getValue();
                        var p = new Array();

                        p.push({ name: 'P0', value: p0 });
                        p.push({ name: 'p0_Name', value: p0_Name });
                        p.push({ name: 'P1', value: p1 });
                        p.push({ name: 'P2', value: p2 });
                        p.push({ name: 'P3', value: p3 });
                        PostForm(T1GetTxt, p);
                    }
                }
            }
        ]
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 't2export',
                id: 't2export',
                name: 't2export',
                text: '匯出',
                handler: function () {
                    if (!Ext.getCmp('t2P0').validate() || !Ext.getCmp('t2P1').validate() || !Ext.getCmp('t2P2').validate()) {
                        Ext.Msg.alert('訊息', '需填[物料分類]、[進貨日期]才能查詢');
                    } else {
                        var p0 = T2Query.getForm().findField('t2P0').getValue();
                        //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
                        var p0_Name = T2Query.getForm().findField('t2P0').getRawValue();
                        //p0_Name = Ext.util.Format.substr(p0_Name, 3, p0_Name.length);
                        var p1 = T2Query.getForm().findField('t2P1').getRawValue();
                        var p2 = T2Query.getForm().findField('t2P2').getRawValue();
                        var p3 = T2Query.getForm().findField('t2P3').getValue()['t2P3'];
                        var p = new Array();

                        p.push({ name: 'FN', value: '中央庫房' + p0_Name + '類' + p1 + '採購月結報表.xls' });
                        p.push({ name: 'P0', value: p0 });
                        p.push({ name: 'P1', value: p1 });
                        p.push({ name: 'P2', value: p2 });
                        p.push({ name: 'P3', value: p3 });
                        PostForm(T2GetExcel, p);
                    }
                }
            },
            {
                text: '列印',
                id: 't2print',
                name: 't2print',
                style: 'margin:0px 5px;',
                handler: function () {
                    if (!Ext.getCmp('t2P0').validate() || !Ext.getCmp('t2P1').validate() || !Ext.getCmp('t2P2').validate()) {
                        Ext.Msg.alert('訊息', '需填[物料分類]、[進貨日期]才能查詢');
                    }
                    else {
                        showReport2();
                    }
                }
            //},
            //{
            //    text: '匯出TXT',
            //    id: 't2exportTXT',
            //    name: 't2exportTXT',
            //    style: 'margin:0px 5px;',
            //    handler: function () {
            //        if (!Ext.getCmp('t2P0').validate() || !Ext.getCmp('t2P1').validate() || !Ext.getCmp('t2P2').validate()) {
            //            Ext.Msg.alert('訊息', '需填[物料分類]、[進貨日期]才能查詢');
            //        } else {
            //            var p0 = T2Query.getForm().findField('t2P0').getValue();
            //            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            //            var p0_Name = T2Query.getForm().findField('t2P0').getRawValue();
            //            //p0_Name = Ext.util.Format.substr(p0_Name, 3, p0_Name.length);
            //            var p1 = T2Query.getForm().findField('t2P1').getRawValue();
            //            var p2 = T2Query.getForm().findField('t2P2').getRawValue();
            //            var p3 = T2Query.getForm().findField('t2P3').getValue()['t2P3'];
            //            var p = new Array();

            //            p.push({ name: 'P0', value: p0 });
            //            p.push({ name: 'p0_Name', value: p0_Name });
            //            p.push({ name: 'P1', value: p1 });
            //            p.push({ name: 'P2', value: p2 });
            //            p.push({ name: 'P3', value: p3 });
            //            PostForm(T2GetTxt, p);
            //        }
            //    }
            }
        ]
    });
    // 衛材--查詢結果資料列表
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
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "廠商碼",
            dataIndex: 'AGEN_NO',            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            style: 'text-align:left',
            align: 'left',
            width: 250
        }, {
            text: "帳號",
            dataIndex: 'AGEN_ACC',
            style: 'text-align:left',
            align: 'left',
            width: 180
        }, {
            text: "100%總價",
            dataIndex: 'FULLSUM',
            style: 'text-align:left',
            align: 'right',
            width: 150
        }, {
            text: "折讓金額",
            dataIndex: 'DISCSUM',
            style: 'text-align:left',
            align: 'right',
            width: 150
        }, {
            text: "支付金額",
            dataIndex: 'PAYSUM',
            style: 'text-align:left',
            align: 'right',
            width: 150
        }],
        listeners: {
            selectionchange: function (model, records) {
                //T1RecLength = records.length;
                //T1LastRec = records[0];
            }
        }
    });
    // 一般物品--查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]     //新增 修改功能畫面
        },
        {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "廠商碼",
            dataIndex: 'AGEN_NO',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            style: 'text-align:left',
            align: 'left',
            width: 250
        }, {
            text: "帳號",
            dataIndex: 'AGEN_ACC',
            style: 'text-align:left',
            align: 'left',
            width: 180
        }, {
            text: "金額",
            dataIndex: 'FULLSUM',
            style: 'text-align:left',
            align: 'right',
            width: 150
        }],
        listeners: {
            selectionchange: function (model, records) {
                //T2RecLength = records.length;
                //T2LastRec = records[0];
            }
        }
    });
    function showReport() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            var tmp_p0_Name = T1Query.getForm().findField('P0').getRawValue();
            //tmp_p0_Name = Ext.util.Format.substr(tmp_p0_Name, 3, tmp_p0_Name.length);
            var np = {
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getRawValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p0_Name=' + tmp_p0_Name + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    function showReport2() {
        if (!win) {
            //取得物料分類下拉選單的選項，並將前3碼截掉(去掉物料代碼，僅留下物料名稱)
            var tmp_p0_Name = T2Query.getForm().findField('t2P0').getRawValue();
            //tmp_p0_Name = Ext.util.Format.substr(tmp_p0_Name, 3, tmp_p0_Name.length);
            var np = {
                p0: T2Query.getForm().findField('t2P0').getValue(),
                p1: T2Query.getForm().findField('t2P1').getRawValue(),
                p2: T2Query.getForm().findField('t2P2').getRawValue(),
                p3: T2Query.getForm().findField('t2P3').getValue()['t2P3']
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl2 + '?p0=' + np.p0 + '&p0_Name=' + tmp_p0_Name + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義畫面 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊

        var TATabs = Ext.widget('tabpanel', {
            listeners: {
                tabchange: function (tabpanel, newCard, oldCard) {
                    switch (newCard.title) {
                        case "衛材":

                            //T1QueryForm.getForm().findField('P0').focus();
                            break;
                        case "一般物品":

                            //T2QueryForm.getForm().findField('P4').focus();
                            //T1QueryForm.getForm().findField('P0').clearInvalid();
                            //T1QueryForm.getForm().findField('P1').clearInvalid();
                            //T1QueryForm.getForm().findField('P2').clearInvalid();
                            //T1QueryForm.getForm().findField('P3').clearInvalid();
                            break;
                    }
                }
            },
            layout: 'fit',
            plain: true,
            border: false,
            resizeTabs: true,       //改變tab尺寸       
            enableTabScroll: true,  //是否允許Tab溢出時可以滾動
            defaults: {
                // autoScroll: true,
                closabel: false,    //tab是否可關閉
                padding: 0,
                split: true
            },
            items: [{
                itemId: 't1Grid',
                title: '衛材',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                title: '一般物品',
                layout: 'border',
                padding: 0,
                split: true,
                region: 'center',
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T2Grid]
            }]
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
                itemId: 't1Form',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [TATabs]
            }
            ]
        });
    //var viewport = Ext.create('Ext.Viewport', {
    //    renderTo: body,
    //    layout: {
    //        type: 'border',
    //        padding: 0
    //    },
    //    defaults: {
    //        split: true
    //    },
    //    items: [{
    //        itemId: 't1Grid',
    //        region: 'center',
    //        layout: 'fit',
    //        collapsible: false,
    //        title: '',
    //        border: false,
    //        items: [T1Grid]
    //    },
    //    ]
    //});

    Ext.getCmp('export').setDisabled(true);
    Ext.getCmp('print').setDisabled(true);
    Ext.getCmp('exportTXT').setDisabled(true);
    Ext.getCmp('t2export').setDisabled(true);
    Ext.getCmp('t2print').setDisabled(true);
});