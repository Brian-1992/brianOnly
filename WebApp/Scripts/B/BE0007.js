Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();
    // var T1Get = '/api/BE0007/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "發票管理維護";

    var T1Rec = 0;
    var T1LastRec = null;
    var isNew = false;
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var ADMIN = "N";
    if (Ext.getUrlParam('ADMIN') != null) ADMIN = Ext.getUrlParam('ADMIN');
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var col_labelWid = 80;
    var col_Wid = 200;

    var mnsetStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1']
    });
    var invoiceStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE']
    });
    var matClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1']
    });



    function getDateRanges() {
        Ext.Ajax.request({
            url: '/api/BE0007/getDateRanges',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts[0];

                    var mnsets = data.etts;
                    for (var i = 0; i < mnsets.length; i++) {
                        mnsetStore.add(mnsets[i]);
                        if (i == 0) {
                            T1Query.getForm().findField('P2').setRawValue(temp.TEXT);
                            T1Query.getForm().findField('P3').setRawValue(temp.VALUE);
                            T1Query.getForm().findField('P4').setRawValue(temp.EXTRA1);
                            MergeForm.getForm().findField('winMergeP1').setRawValue(temp.EXTRA1);
                            MergeForm.getForm().findField('winMergeP2').setRawValue(temp.TEXT);
                            MergeForm.getForm().findField('winMergeP3').setRawValue(temp.VALUE);
                            ExportForm.getForm().findField('winExportP1').setRawValue(temp.EXTRA1);
                            ExportForm.getForm().findField('winExportP2').setRawValue(temp.EXTRA1);
                            ExportForm.getForm().findField('winExportP1').setValue(temp.EXTRA1);
                            ExportForm.getForm().findField('winExportP2').setValue(temp.EXTRA1);
                            getInvoiceCombo();
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getMatclassCombo() {
        Ext.Ajax.request({
            url: '/api/BE0007/GetMatclassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var temp = data.etts[0];
                    matClassStore.removeAll();
                    matClassStore.add({ VALUE: '', TEXT: '全部' });

                    var matClasses = data.etts;
                    for (var i = 0; i < matClasses.length; i++) {
                        matClassStore.add(matClasses[i]);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function getInvoiceCombo() {
        Ext.Ajax.request({
            url: '/api/BE0007/getInvoiceCombo',
            params: {
                p2: T1Query.getForm().findField('P2').rawValue,
                p3: T1Query.getForm().findField('P3').rawValue,
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    invoiceStore.removeAll();
                    var temp = data.etts;
                    
                    var invoice = data.etts;
                    for (var i = 0; i < invoice.length; i++) {
                        invoiceStore.add(invoice[i]);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function setComboData() {
        getDateRanges();
        getMatclassCombo();
    }
    setComboData();
    

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

    // 查詢欄位
    var mLabelWidth = 80;
    var mWidth = 230;
    var AgennoComboGet = '/api/BE0007/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo_1', {
        name: 'P0',
        id: 'P0',
        fieldLabel: '廠商代碼',
        allowBlank: true,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: mLabelWidth,
        width: 300,
        //margin: '1 0 1 0',
        padding: '0 1 0 1',
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P0').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });
    var T1QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        readOnly: true,
        allowBlank: false,
        fieldCls: 'required',
        width: 220,
        matchFieldWidth: false,
        listConfig: { width: 180 },
        margin: '0 0 10 0',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/BE0007/GetMmCodeCombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'DISC_CPRICE', 'M_CONTPRICE', 'M_AGENNO', 'AGEN_NAMEC', 'UNI_NO', 'M_NHIKEY'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                
                T1Form.getForm().findField('MMNAME_C').setValue(r.get('MMNAME_C'));
                T1Form.getForm().findField('AGEN_NO').setValue(r.get('M_AGENNO'));
                T1Form.getForm().findField('AGEN_NAMEC').setValue(r.get('AGEN_NAMEC'));
                T1Form.getForm().findField('UNI_NO').setValue(r.get('UNI_NO'));
                T1Form.getForm().findField('M_NHIKEY').setValue(r.get('M_NHIKEY'));
                T1Form.getForm().findField('PO_PRICE').setValue(r.get('M_CONTPRICE'));
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            margin: '2 4 0 0'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                T1FormAgenno,
                //{
                //    xtype: 'textfield',
                //    fieldLabel: '發票號碼',
                //    name: 'P1',
                //    id: 'P1',
                //    enforceMaxLength: true,
                //    maxLength: 10,
                //    width: 230
                //},
                {
                    xtype: 'combo',
                    store: invoiceStore,
                    name: 'P1',
                    id: 'P1',
                    fieldLabel: '發票號碼',
                    displayField: 'VALUE',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: true,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div></tpl>'

                }, 
                {
                    xtype: 'combo',
                    store: matClassStore,
                    name: 'P5',
                    id: 'P5',
                    fieldLabel: '物料類別',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'

                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: mnsetStore,
                    name: 'P4',
                    id: 'P4',
                    fieldLabel: '月結年月',
                    displayField: 'EXTRA1',
                    valueField: 'EXTRA1',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false,
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    multiSelect: false,
                    labelWidth: mLabelWidth,
                    width: 300,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{EXTRA1} ({TEXT} ~ {VALUE})&nbsp;</div></tpl>',
                    listeners: {
                        change: function (combo, value) {
                            T1Query.getForm().findField('P2').setRawValue(combo.selection.data.TEXT);
                            T1Query.getForm().findField('P3').setRawValue(combo.selection.data.VALUE);
                            getInvoiceCombo();
                        }
                    }
                }, {
                    xtype: 'displayfield',
                    name: 'P2',
                    id: 'P2',
                    fieldLabel: '進貨日期',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 135
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P3',
                    id: 'P3',
                    labelAlign: 'right',
                    labelWidth: 15,
                    width: 70
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '2 4 0 20',
                    handler: function () {
                        isNew = false;
                        T1Load();
                        msglabel('訊息區:');
                        btnControl('2');
                    }

                }, {
                    xtype: 'button',
                    text: '清除',
                    margin: '2 4 0 0',
                    handler: function () {
                        isNew = false;
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                        //btnControl('2');
                    }
                }
            ]
        }]
    });
    function btnControl(id) {
        isNew = false;
        if (id == '1') //點選T1Grid一筆資料的動作
        {
            Ext.getCmp('btnT1add').setDisabled(false);
            Ext.getCmp('btnT1edit').setDisabled(false);
            Ext.getCmp('btnT1Del').setDisabled(false);
            Ext.getCmp('btnT1merge').setDisabled(false);
            Ext.getCmp('excel').setDisabled(false);
        }
        else if (id == '2')  //點選 查詢btn 動作
        {
            Ext.getCmp('btnT1add').setDisabled(false);
            Ext.getCmp('btnT1edit').setDisabled(true);
            Ext.getCmp('btnT1Del').setDisabled(true);
            Ext.getCmp('btnT1merge').setDisabled(false);
            Ext.getCmp('excel').setDisabled(false);
        }
    }

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'INVOICE', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'UNI_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'EXTRA_DISC_AMOUNT', type: 'string' },
            { name: 'MORE_DISC_AMOUNT', type: 'string' },
            { name: 'TOTAL_AMT', type: 'string' },
            { name: 'PO_NO', type: 'string' },
            { name: 'PO_PRICE', type: 'string' },
            { name: 'SPMMCODE', type: 'string' },
            { name: 'NHI_PRICE', type: 'string' },
            { name: 'TRANSNO', type: 'string' },
            { name: 'IS_INCLUDE_TAX', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'DATA_YM', type: 'string' },
            { name: 'INVOICE_TYPE', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'INVOICE_DT', direction: 'ASC' }, { property: 'MMCODE', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0007/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),   //廠商代碼
                    p1: T1Query.getForm().findField('P1').rawValue,     //發票號碼
                    p2: T1Query.getForm().findField('P2').rawValue,     //進貨日期 起
                    p3: T1Query.getForm().findField('P3').rawValue,     //進貨日期 迄
                    p4: T1Query.getForm().findField('P4').rawValue,   //月結年月
                    p5: T1Query.getForm().findField('P5').getValue()    //物料類別
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '新增',
                name: 'btnT1add', id: 'btnT1add',
                disabled: true,
                handler: function () {
                    T1Set = '/api/BE0007/CreateM';
                    msglabel('訊息區:');
                    setFormT1("I", '新增');
                    //if (T1LastRec) { }
                    // else {
                    //    btnControl('2'); T1Cleanup(); //viewport.down('#form').collapse();
                    //}
                }
            }, {
                text: '修改',
                name: 'btnT1edit', id: 'btnT1edit',
                disabled: true,
                handler: function () {
                    T1Set = '/api/BE0007/UpdateM';
                    msglabel('訊息區:');
                    if (T1LastRec) {
                        setFormT1("U", '修改');
                    }
                    //else {
                    //    btnControl('2'); T1Cleanup(); //viewport.down('#form').collapse();
                    //}//
                }
            }, {
                text: '刪除',
                name: 'btnT1Del', id: 'btnT1Del',
                disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let po_no = '';
                        let mmcode = '';
                        let transno = '';
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('PO_NO') + '」<br>';
                            po_no += item.get('PO_NO') + ',';
                            mmcode += item.get('MMCODE') + ',';
                            transno += item.get('TRANSNO') + ',';
                        })

                        Ext.MessageBox.confirm('提醒', '是否確定[刪除]?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BE0007/UpdateDEL',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no,
                                        MMCODE: mmcode,
                                        TRANSNO: transno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            msglabel('刪除成功');
                                        }
                                        else
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }, {
                text: '彙整',
                name: 'btnT1merge', id: 'btnT1merge',
                disabled: true,
                handler: function () {
                    showWinMerge();
                }
            }, {
                text: '匯出',
                name: 'excel', id: 'excel',
                disabled: true,
                handler: function () {
                    showWinExport();
                }
            }, {
                text: '列印廠商發票對帳明細(不分頁)',
                name: 'print', id: 'print',
                handler: function () {
                    showReport('/Report/B/BE0007.aspx', "1");
                }
            }, {
                text: '列印廠商發票對帳明細(分頁)',
                name: 'print2', id: 'print2',
                handler: function () {
                    showReport('/Report/B/BE0007.aspx', "2");
                }
            }
            //,
            //{
            //    text: '列印廠商發票對帳明細(A3)',
            //    name: 'print3', id: 'print3',
            //    handler: function () {
            //        showReport('/Report/B/BE0007.aspx', "3");
            //    }
            //},
            //{
            //    text: '列印廠商發票對帳明細(A3)',
            //    name: 'print4', id: 'print4',
            //    handler: function () {
            //        Ext.Msg.show({
            //            title: '選擇列印樣式',
            //            message: '選擇列印樣式:',
            //            buttons: Ext.Msg.YESNO,
            //            buttonText: {
            //                yes: '橫式',
            //                no: '直式'
            //            },
            //            icon: Ext.Msg.QUESTION,
            //            fn: function (btn) {
            //                if (btn === 'yes') {
            //                    showReport('/Report/B/BE0007.aspx', "3");
            //                } else {
            //                    showReport('/Report/B/BE0007.aspx', "4");
            //                }
            //            }
            //        });
            //    }
            //}
            , {
                text: '列印廠商發票對帳明細(A3)',
                name: 'print4', id: 'print4',
                handler: function () {
                    showWinPrintA3();
                }
            }
        ]
    });


    function T1Load() {
        T1Tool.moveFirst();
    }

    //按下新增(I)/修改(U) btn
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        Ext.getCmp('submit').show();
        Ext.getCmp('cancel').show();

        if (x === "I") //新增
        {
            viewport.down('#form').expand();
            isNew = true;
            var r = Ext.create(T1Model);
            T1Form.reset();
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("PO_NO");
            f.findField('INVOICE').setValue('');
            f.findField('INVOICE_DT').setValue('');
            f.findField('DATA_YM').setValue(T1Query.getForm().findField('P4').rawValue); //資料年月
            f.findField('IS_INCLUDE_TAX').setValue('Y'); //含稅
            //f.findField('MEMO').setValue('');
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('PO_NO').setReadOnly(false);            //訂單號碼
            f.findField('DELI_QTY').setReadOnly(false);         //藥材數量
            f.findField('MMCODE').setReadOnly(false);
            f.findField('DELI_QTY').setValue(1);
            f.findField('EXTRA_DISC_AMOUNT').setValue(0);
            f.findField('MORE_DISC_AMOUNT').setValue(0);
        }
        else if (x === "U") //修改
        {
            isNew = false;
            if (T1LastRec) {
                f.findField('PO_NO').setReadOnly(true);            //訂單號碼
                f.findField('DELI_QTY').setReadOnly(true);         //藥材數量
            }
            u = f.findField('PO_NO');
        }
        f.findField('x').setValue(x);
        f.findField('INVOICE').setReadOnly(false);          //發票號碼
        f.findField('INVOICE_DT').setReadOnly(false);       //發票日期
        f.findField('INVOICE_TYPE').setReadOnly(false);     //發票類別
        f.findField('PO_PRICE').setReadOnly(false);      //單價
        f.findField('PO_AMT').setReadOnly(false);           //小計
        f.findField('EXTRA_DISC_AMOUNT').setReadOnly(false);//折讓金額
        f.findField('MORE_DISC_AMOUNT').setReadOnly(false); //優惠金額

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }
    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "資料年月",
            dataIndex: 'DATA_YM',
            width: 80,
            align: 'center',
            style: 'text-align:left'
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 80,
            align: 'center'
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 100,
            align: 'center'
        }, {
            text: "發票類別",
            dataIndex: 'INVOICE_TYPE',
            width: 80,
            align: 'center'
        }, {
            text: "藥材代碼",
            dataIndex: 'MMCODE',
            width: 100
        }, {
            text: "健保代碼",
            dataIndex: 'M_NHIKEY',
            width: 120
        }, {
            text: "藥材名稱",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "特材號碼",
            dataIndex: 'SPMMCODE',
            width: 200
        }, {
            text: "健保價",
            dataIndex: 'NHI_PRICE',
            width: 200
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            width: 80,
            align: 'center'
        }, {
            text: "廠商統編",
            dataIndex: 'UNI_NO',
            width: 80
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 200
        }, {
            text: "單價",
            dataIndex: 'PO_PRICE',
            width: 80,
            align: 'right',
            style: 'text-align:left',
        }, {
            text: "藥材數量",
            dataIndex: 'DELI_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "小計",
            dataIndex: 'PO_AMT',
            width: 80,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "折讓金額",
            dataIndex: 'EXTRA_DISC_AMOUNT',
            width: 80,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "優惠金額",
            dataIndex: 'MORE_DISC_AMOUNT',
            width: 80,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "總金額",
            dataIndex: 'TOTAL_AMT',
            width: 80,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "含稅",
            dataIndex: 'IS_INCLUDE_TAX',
            width: 60,
            align: 'center'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 120
        }, {
            text: "最後更新日",
            dataIndex: 'UPDATE_TIME',
            width: 90,
            align: 'center'
        }, {
            text: "資料流水號",
            dataIndex: 'TRANSNO',
            width: 120
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'INVOICE');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }

                // T61LastRec = record;
                T1cell = 'cell';
                //
                T1LastRec = Ext.clone(record);
                //if (T1LastRec.data.CHK_WH_GRADE == '2' &&
                //    T1LastRec.data.CHK_WH_KIND == '0') {
                //    clearT31QueryFilter();
                //    return;
                //}
                T1Form.loadRecord(T1LastRec);

                Ext.getCmp('submit').hide();
                Ext.getCmp('cancel').hide();
            },
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        //cls: 'T1b',
        title: '',
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: col_labelWid,
            width: col_Wid,
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'container',
                layout: {
                    type: 'table',
                    columns: 2,
                },
                items: [
                    {
                        name: 'x',
                        xtype: 'hidden'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '資料年月',
                        name: 'DATA_YM',
                        submitValue: true,
                        colspan: 2
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '發票日期',
                        name: 'INVOICE_DT',
                        enforceMaxLength: true,
                        readOnly: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        submitValue: true
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '發票號碼',
                        name: 'INVOICE',
                        enforceMaxLength: true,
                        maxLength: 10,
                        regexText: '請輸入正確發票號碼',
                        regex: /[A-Z0-9]/, // 用正規表示式限制可輸入內容
                        readOnly: true,
                        allowBlank: false,
                        fieldCls: 'required',
                        submitValue: true,
                        listeners: {
                            blur: function () {
                                T1Form.getForm().findField('INVOICE').setValue(Ext.util.Format.uppercase(this.value));
                            }
                        }
                    }, {
                        xtype: 'radiogroup',
                        fieldLabel: '發票類別',
                        items: [
                            { boxLabel: '銷貨', name: 'INVOICE_TYPE', inputValue: '銷貨', width: 70, checked: true },
                            { boxLabel: '退貨', name: 'INVOICE_TYPE', inputValue: '退貨', width: 70, },
                            { boxLabel: '非發票', name: 'INVOICE_TYPE', inputValue: '非發票', width: 120, },
                        ],
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                //changeApplyKind(newValue['P2'], false);
                            }
                        },
                        colspan: 2
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '訂單號碼',
                        name: 'PO_NO',
                        enforceMaxLength: true,
                        //maxLength: 10,
                        //regexText: '請輸入正確發票號碼',
                        //regex: /[A-Z0-9]/, // 用正規表示式限制可輸入內容
                        readOnly: true,
                        allowBlank: true,
                        //fieldCls: 'required',
                        submitValue: true,
                        listeners: {
                            //change: function (field, newValue, oldValue) {
                            //    if (isNew == true && newValue != null && newValue != '') {
                            //        //T1Form.getForm().findField('INVOICE').setValue(Ext.util.Format.uppercase(this.value));
                            //        Ext.Ajax.request({
                            //            url: '/api/BE0007/GetInsertData',
                            //            method: reqVal_p,
                            //            params: {
                            //                PO_NO: newValue
                            //            },
                            //            success: function (response) {
                            //                var data = Ext.decode(response.responseText);
                            //                if (data.success) {
                            //                    //依據訂單號碼取得相關資料 藥材代碼$藥材名稱$單價$健保代碼$廠商代碼$廠商名稱$廠商統編
                            //                    var f = T1Form.getForm();
                            //                    if (data.msg == null) {
                            //                        f.findField('MMCODE').setValue('');//藥材代碼
                            //                        f.findField('MMNAME_C').setValue('');//藥材名稱
                            //                        f.findField('PO_PRICE').setValue('');//單價
                            //                        f.findField('M_NHIKEY').setValue('');//健保代碼
                            //                        f.findField('AGEN_NO').setValue('');//廠商代碼
                            //                        f.findField('AGEN_NAMEC').setValue('');//廠商名稱
                            //                        f.findField('UNI_NO').setValue('');//廠商統編
                            //                        // Ext.MessageBox.alert('警告', '查無此訂單號碼');
                            //                    }
                            //                    else {
                            //                        var restr = data.msg.split('$');
                            //                        f.findField('MMCODE').setValue(restr[0].toString());//藥材代碼
                            //                        f.findField('MMNAME_C').setValue(restr[1].toString());//藥材名稱
                            //                        f.findField('PO_PRICE').setValue(restr[2].toString());//單價
                            //                        f.findField('M_NHIKEY').setValue(restr[3].toString());//健保代碼
                            //                        f.findField('AGEN_NO').setValue(restr[4].toString());//廠商代碼
                            //                        f.findField('AGEN_NAMEC').setValue(restr[5].toString());//廠商名稱
                            //                        f.findField('UNI_NO').setValue(restr[6].toString());//廠商統編
                            //                    }
                            //                }
                            //                else {
                            //                    var f = T1Form.getForm();
                            //                    f.findField('MMCODE').setValue('');//藥材代碼
                            //                    f.findField('MMNAME_C').setValue('');//藥材名稱
                            //                    f.findField('PO_PRICE').setValue('');//單價
                            //                    f.findField('M_NHIKEY').setValue('');//健保代碼
                            //                    f.findField('AGEN_NO').setValue('');//廠商代碼
                            //                    f.findField('AGEN_NAMEC').setValue('');//廠商名稱
                            //                    f.findField('UNI_NO').setValue('');//廠商統編
                            //                    Ext.MessageBox.alert('錯誤', data.msg);
                            //                }
                            //            },
                            //            failure: function (response) {
                            //                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            //            }
                            //        });
                            //    }
                            //}
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '健保代碼',
                        name: 'M_NHIKEY',
                        submitValue: true
                    },
                    //{
                    //    xtype: 'displayfield',
                    //    fieldLabel: '藥材代碼',
                    //    name: 'MMCODE',
                    //    submitValue: true
                    //},
                    T1QueryMMCode,
                    {
                        xtype: 'displayfield',
                        fieldLabel: '資料流水號',
                        name: 'TRANSNO',
                        submitValue: true
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '藥材名稱',
                        name: 'MMNAME_C',
                        width: 350,
                        colspan: 2
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商代碼',
                        name: 'AGEN_NO'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商統編',
                        name: 'UNI_NO'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '廠商名稱',
                        name: 'AGEN_NAMEC',
                        width: 350,
                        colspan: 2
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '含稅',
                        name: 'IS_INCLUDE_TAX'
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '單價',
                        name: 'PO_PRICE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 0,
                        allowBlank: false,
                        fieldCls: 'required',
                        labelAlign: 'right',
                        decimalPrecision: 4,
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                
                                countPoAmt();
                                ///changeChkPeriod(newValue['CHK_PERIOD']);
                            }
                        }
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '藥材數量',
                        name: 'DELI_QTY',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 1,
                        allowBlank: false,
                        fieldCls: 'required',
                        labelAlign: 'right',
                        decimalPrecision: 0,
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                
                                countPoAmt();
                                ///changeChkPeriod(newValue['CHK_PERIOD']);
                            }
                        }
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '小計',
                        name: 'PO_AMT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 0,
                        allowBlank: false,
                        fieldCls: 'required',
                        labelAlign: 'right',
                        decimalPrecision: 4,
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                
                                countToyalAmt();
                                ///changeChkPeriod(newValue['CHK_PERIOD']);
                            }
                        }
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '折讓金額',
                        name: 'EXTRA_DISC_AMOUNT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 0,
                        allowBlank: false,
                        fieldCls: 'required',
                        labelAlign: 'right',
                        decimalPrecision: 0,
                        listeners: {
                            change: function (field, newValue, oldValue) {
                                
                                countToyalAmt();
                                ///changeChkPeriod(newValue['CHK_PERIOD']);
                            }
                        }
                    }, {
                        xtype: 'numberfield',
                        fieldLabel: '優惠金額',
                        name: 'MORE_DISC_AMOUNT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        minValue: 0,
                        allowBlank: false,
                        fieldCls: 'required',
                        labelAlign: 'right',
                        decimalPrecision: 0,
                        listeners: {
                            change: function (field, newValue, oldValue, ctrs) {
                                countToyalAmt();
                                ///changeChkPeriod(newValue['CHK_PERIOD']);
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '總金額',
                        name: 'TOTAL_AMT',
                        submitValue: true
                    }, {
                        name: 'UPDATE_TIME',
                        xtype: 'displayfield',
                        fieldLabel: '最後更新日',
                        submitValue: true
                    }
                ]
            }
        ],
        buttons: [
            {
                id: 'submit', itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (T1Form.getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit;//= viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                                isNew = false;
                            }
                        }
                        );

                        //if ((this.up('form').getForm().findField('INVOICE').getValue() == '' || this.up('form').getForm().findField('INVOICE').getValue() == null) &&
                        //    (this.up('form').getForm().findField('INVOICE_DT').getValue() == '' || this.up('form').getForm().findField('INVOICE_DT').getValue() == null))
                        //    Ext.Msg.alert('提醒', '必須輸入發票號碼或發票日期');
                        //else {
                        //    if (this.up('form').getForm().findField('CKIN_QTY').getValue() == '0') {
                        //        Ext.Msg.alert('提醒', '發票驗證數量不得為0');
                        //    }
                        //    else {

                        //    }
                        //}
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel('訊息區:輸入資料格式有誤');
                    }
                }
            }, {
                id: 'cancel', itemId: 'cancel', text: '取消', hidden: true, handler: function () {
                    T1Cleanup();
                    isNew = false;
                }
            },
        ]
    });

    //小計: 單價*藥材數量
    function countPoAmt() {
        var f = T1Form.getForm();
        var po_price = f.findField('PO_PRICE').getValue();//單價
        var deli_qty = f.findField('DELI_QTY').getValue();      //藥材數量
        var po_amt = Math.round(po_price * deli_qty);
        f.findField('PO_AMT').setValue(po_amt);
    }

    //總金額: 小計-折扣金額-優惠金額
    function countToyalAmt() {
        var f = T1Form.getForm();
        
        var po_amt = f.findField('PO_AMT').getValue();                 //小計
        var extra_disc_amount = f.findField('EXTRA_DISC_AMOUNT').getValue();//折讓金額
        var more_disc_amount = f.findField('MORE_DISC_AMOUNT').getValue();  //優惠金額
        var total_amt = po_amt - extra_disc_amount - more_disc_amount;
        f.findField('TOTAL_AMT').setValue(total_amt);

        //var disc_cprice = f.findField('DISC_CPRICE').getValue();
        //var extra_disc_amount = f.findField('EXTRA_DISC_AMOUNT').getValue();
        //var more_disc_amount = f.findField('MORE_DISC_AMOUNT').getValue();
        //var should_pay = po_price * deli_qty;
        //var contract_disc_amount = po_price * deli_qty - disc_cprice * deli_qty;
        //var actual_pay = Math.round(disc_cprice * deli_qty) - extra_disc_amount - more_disc_amount;
        //f.findField('ACTUAL_PAY').setValue(actual_pay);
    }

    //點選T1Grid一筆資料的動作
    function setFormT1a() {
        
        if (T1LastRec) {
            btnControl('1');
            isNew = false;
            
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('INVOICE_DT').setReadOnly(true);       //發票日期
            f.findField('INVOICE_TYPE').setReadOnly(true);     //發票類別
            f.findField('PO_NO').setReadOnly(true);            //訂單號碼
            f.findField('DELI_QTY').setReadOnly(true);         //藥材數量
            f.findField('INVOICE').setReadOnly(true);          //發票號碼
            f.findField('INVOICE_DT').setReadOnly(true);       //發票日期
            f.findField('INVOICE_TYPE').setReadOnly(true);     //發票類別
            f.findField('PO_PRICE').setReadOnly(true);      //單價
            f.findField('PO_AMT').setReadOnly(true);           //小計
            f.findField('EXTRA_DISC_AMOUNT').setReadOnly(true);//折讓金額
            f.findField('MORE_DISC_AMOUNT').setReadOnly(true); //優惠金額


            //T1F1 = f.findField('PO_NO').getValue();
            //T1F2 = f.findField('MMCODE').getValue();
            //T1F3 = f.findField('TRANSNO').getValue();
            //T1F4 = f.findField('CKSTATUS').getValue();
            //var u = f.findField('INVOICE');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            //if (T1F4 == '驗退') {
            //    Ext.getCmp('btnT1back').setDisabled(true);
            //} if (T1F4 == '已驗證') {
            //    Ext.getCmp('btnT1edit').setDisabled(true);
            //    //Ext.getCmp('btnT1back').setDisabled(true);
            //    //Ext.getCmp('btnT1apply').setDisabled(true);
            //    Ext.getCmp('btnT1Del').setDisabled(true);
            //} else {
            //    Ext.getCmp('btnT1edit').setDisabled(false);
            //    //Ext.getCmp('btnT1back').setDisabled(false);
            //    //Ext.getCmp('btnT1apply').setDisabled(false);
            //    Ext.getCmp('btnT1Del').setDisabled(false);
            //}
            //countActualPay();
            Ext.getCmp('submit').hide();
            Ext.getCmp('cancel').hide();
            viewport.down('#form').expand();
        }
        else {
            T1Form.getForm().reset();
        }
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
                    var data = Ext.decode(action.response.responseText);
                    if (data.success) {
                        if (data.msg.toString() == "") {
                            switch (f2.findField("x").getValue()) {

                                case "I":
                                    // 新增後,將key代入查詢條件,只顯示剛新增的資料
                                    //var v = action.result.etts[0];
                                    //T1Query.getForm().findField('P0').setValue(v.PO_NO);
                                    T1Query.getForm().findField('P1').setValue(T1Form.getForm().findField('INVOICE').getValue());
                                    T1Load();
                                    msglabel('訊息區:資料新增成功');
                                    isNew = false;
                                    break;
                                case "U":
                                    //var v = action.result.etts[0];
                                    //r.set(v);
                                    //r.commit();
                                    T1Query.getForm().findField('P1').setValue(T1Form.getForm().findField('INVOICE').getValue());
                                    T1Load();
                                    msglabel('訊息區:資料修改成功');
                                    isNew = false;
                                    break;
                            }
                            T1Cleanup();
                        } else {
                            Ext.MessageBox.alert('警示', data.msg);
                        }
                    }
                    else
                        Ext.MessageBox.alert('錯誤', data.msg);

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
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').collapse();
        //btnControl('2');
    }

    var MergeForm = Ext.widget({
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
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        store: mnsetStore,
                        name: 'winMergeP1',
                        id: 'winMergeP1',
                        fieldLabel: '月結年月',
                        displayField: 'EXTRA1',
                        valueField: 'EXTRA1',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        labelWidth: 80,
                        width: 250,
                        matchFieldWidth: false,
                        listConfig: { width: 230 },
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{EXTRA1} ({TEXT} ~ {VALUE})&nbsp;</div></tpl>',
                        listeners: {
                            change: function (combo, value) {
                                
                                MergeForm.getForm().findField('winMergeP2').setRawValue(combo.selection.data.TEXT);
                                MergeForm.getForm().findField('winMergeP3').setRawValue(combo.selection.data.VALUE);
                            }
                        }
                    }
                ]
            }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'displayfield',
                        name: 'winMergeP2',
                        id: 'winMergeP2',
                        fieldLabel: '進貨日期',
                        //labelAlign: 'right',
                        labelWidth: 80,
                        width: 135
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '至',
                        labelSeparator: '',
                        name: 'winMergeP3',
                        id: 'winMergeP3',
                        //labelAlign: 'right',
                        labelWidth: 10,
                        width: 70
                    }
                ]
            }
        ],
        buttons: [{
            itemId: 'T3Submit', text: '儲存', handler: function () {
                if (this.up('form').getForm().isValid()) {

                    Ext.Ajax.request({
                        url: '/api/BE0007/CheckHasEmptyInvoice',
                        method: reqVal_p,
                        params: {
                            start_date: MergeForm.getForm().findField('winMergeP2').rawValue,
                            end_date: MergeForm.getForm().findField('winMergeP3').rawValue
                        },
                        //async: true,
                        success: function (response) {
                            
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                Ext.MessageBox.confirm('轉入發票', '有無發票資訊之資料，是否確定彙整?<br>(僅彙整有發票資訊者)', function (btn, text) {
                                    if (btn === 'yes') {
                                        Ext.Ajax.request({
                                            url: '/api/BE0007/Merge',
                                            method: reqVal_p,
                                            params: {
                                                p0: MergeForm.getForm().findField('winMergeP1').rawValue,
                                                p1: MergeForm.getForm().findField('winMergeP2').rawValue,
                                                p2: MergeForm.getForm().findField('winMergeP3').rawValue
                                            },
                                            //async: true,
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    hideWinMerge();
                                                    Ext.MessageBox.alert('訊息', '轉入發票成功');
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
                            else {
                                Ext.MessageBox.confirm('轉入發票', '是否確定轉入發票?', function (btn, text) {
                                    if (btn === 'yes') {
                                        Ext.Ajax.request({
                                            url: '/api/BE0007/Merge',
                                            method: reqVal_p,
                                            params: {
                                                p0: MergeForm.getForm().findField('winMergeP1').rawValue,
                                                p1: MergeForm.getForm().findField('winMergeP2').rawValue,
                                                p2: MergeForm.getForm().findField('winMergeP3').rawValue
                                            },
                                            //async: true,
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    hideWinMerge();
                                                    Ext.MessageBox.alert('訊息', '轉入發票成功');
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
                            // Ext.MessageBox.alert('錯誤', data.msg);
                        },
                        failure: function (response) {
                            
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });




                }
            }
        },
        {
            itemId: 'cancel', text: '取消', handler: hideWinMerge
        }]
    });

    var winMerge;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!winMerge) {
        winMerge = Ext.widget('window', {
            title: '轉入發票',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: MergeForm,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }

    function showWinMerge() {
        if (winMerge.hidden) {
            winMerge.show();
        }
    }
    function hideWinMerge() {
        if (!winMerge.hidden) {
            winMerge.hide();
        }
    }



    var ExportForm = Ext.widget({
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
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        store: mnsetStore,
                        name: 'winExportP1',
                        id: 'winExportP1',
                        fieldLabel: '月結年月',
                        displayField: 'EXTRA1',
                        valueField: 'EXTRA1',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        labelWidth: 70,
                        width: 150,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{EXTRA1}</div></tpl>'
                    },
                    {
                        xtype: 'combo',
                        store: mnsetStore,
                        name: 'winExportP2',
                        id: 'winExportP2',
                        fieldLabel: '至',
                        labelSeparator: '',
                        displayField: 'EXTRA1',
                        valueField: 'EXTRA1',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        padding: '0 0 0 4',
                        labelWidth: 10,
                        width: 90,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{EXTRA1}</div></tpl>'
                    }
                ]
            }
        ],
        buttons: [{
            itemId: 'TExportSubmit', text: '確定', handler: function () {
                if (this.up('form').getForm().isValid()) {
                    var p = new Array();
                    //p.push({ name: 'p1', value: ExportForm.getForm().findField('winExportP1').getValue() });
                    //p.push({ name: 'p2', value: ExportForm.getForm().findField('winExportP2').getValue() });

                    // 2023/11/27 調整成跟 grid 相同的結果
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });   //廠商代碼
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });     //發票號碼
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue });     //進貨日期 起
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').rawValue });     //進貨日期 迄
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').rawValue });     //月結年月
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });   //物料類別

                    PostForm('/api/BE0007/GetExcel', p);
                    hideWinExport();
                }
            }
        },
        {
            itemId: 'cancel', text: '取消', handler: hideWinExport
        }]
    });

    var winExport;
    var winActWidth4 = 320;
    var winActHeight4 = 150;
    if (!winExport) {
        winExport = Ext.widget('window', {
            title: '匯出發票',
            closeAction: 'hide',
            width: winActWidth4,
            height: winActHeight4,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: ExportForm,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth4 > 0) ? winActWidth4 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight4 > 0) ? winActHeight4 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth4 = width;
                    winActHeight4 = height;
                }
            }
        });
    }

    function showWinExport() {
        if (winExport.hidden) {
            winExport.show();
        }
    }
    function hideWinExport() {
        if (!winExport.hidden) {
            winExport.hide();
        }
    }

    var PrintA3Store1 = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '1', TEXT: '直式' },
            { VALUE: '2', TEXT: '橫式' }
        ]
    });

    var PrintA3Store2 = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { VALUE: '1', TEXT: '分頁' },
            { VALUE: '2', TEXT: '不分頁' }
        ]
    });

    var PrintForm = Ext.widget({
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
        autoScroll: false,
        items: [
            {
                xtype: 'panel',
                border: false,
                layout: 'vbox',
                items: [
                    {
                        xtype: 'combo',
                        store: PrintA3Store1,
                        name: 'winPrintP1',
                        id: 'winPrintP1',
                        fieldLabel: '列印樣式',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        labelWidth: 70,
                        width: 150,
                        value: '1',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}</div></tpl>'
                    },
                    {
                        xtype: 'combo',
                        store: PrintA3Store2,
                        name: 'winPrintP2',
                        id: 'winPrintP2',
                        fieldLabel: '是否分頁:',
                        labelSeparator: '',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        queryMode: 'local',
                        anyMatch: true,
                        allowBlank: false,
                        typeAhead: true,
                        forceSelection: true,
                        triggerAction: 'all',
                        multiSelect: false,
                        labelWidth: 70,
                        width: 150,
                        value: '1',
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}</div></tpl>'
                    }
                ]
            }
        ],
        buttons: [{
            text: '確定', handler: function () {
                var a = PrintForm.getForm().findField('winPrintP1').getValue();
                var b = PrintForm.getForm().findField('winPrintP2').getValue();
                var _prtStyle = '';

                //直式
                if (a == 1) {
                    //分頁
                    if (b == 1) {
                        showReport('/Report/B/BE0007.aspx', "5");
                    }
                    else {
                        showReport('/Report/B/BE0007.aspx', "4");
                    }
                }
                else //橫式
                {
                    //分頁
                    if (b == 1) {
                        showReport('/Report/B/BE0007.aspx', "6");

                    }
                    else {
                        showReport('/Report/B/BE0007.aspx', "3");

                    }
                }

                hideWinPrintA3();
                showReport(url, prtStyle);
            }
        }        ,
        {
            text: '取消', handler: hideWinPrintA3
        }]
    });


var winPrintA3;
var winActWidth5 = 250;
var winActHeight5 = 200;
if (!winPrintA3) {
    winPrintA3 = Ext.widget('window', {
        title: '列印',
        closeAction: 'hide',
        width: 250,
        height: 200,
        layout: 'fit',
        resizable: true,
        modal: true,
        constrain: true,
        items: PrintForm,
        listeners: {
            move: function (xwin, x, y, eOpts) {
                xwin.setWidth((viewport.width - winActWidth5 > 0) ? winActWidth5 : viewport.width - 36);
                xwin.setHeight((viewport.height - winActHeight5 > 0) ? winActHeight5 : viewport.height - 36);
            },
            resize: function (xwin, width, height) {
                winActWidth5 = width;
                winActHeight5 = height;
            }
        }
    });
}


function showWinPrintA3() {
    if (winPrintA3.hidden) {
        winPrintA3.show();
    }
}
function hideWinPrintA3() {
    if (!winPrintA3.hidden) {
        winPrintA3.hide();
    }
}




function showReport(url, prtStyle) {
    if (!win) {

        var winform = Ext.create('Ext.form.Panel', {
            id: 'iframeReport',
            //height: '100%',
            //width: '100%',
            layout: 'fit',
            closable: false,
            html: '<iframe src="' + url + '?prtStyle=' + prtStyle
            + '&p0=' + T1Query.getForm().findField('P0').getValue()
            + '&p1=' + T1Query.getForm().findField('P1').rawValue
            + '&p2=' + T1Query.getForm().findField('P2').rawValue
            + '&p3=' + T1Query.getForm().findField('P3').rawValue
            + '&p4=' + T1Query.getForm().findField('P4').rawValue
            + '&p5=' + T1Query.getForm().findField('P5').getValue()
            + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
            buttons: [{
                text: '關閉',
                handler: function () {
                    this.up('window').destroy();
                }
            }]
        });
        var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
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
    items: [{
        itemId: 't1Grid',
        region: 'center',
        layout: 'fit',
        collapsible: false,
        title: '',
        border: false,
        items: [T1Grid]
    }
        ,
    {
        itemId: 'form',
        region: 'east',
        collapsible: true,
        floatable: true,
        width: '40%',
        title: '瀏覽',
        border: false,
        collapsed: true,
        layout: {
            type: 'fit',
            padding: 5,
            align: 'stretch'
        },
        items: [T1Form]
    }

    ]
});
    //T1Load(); // 進入畫面時自動載入一次資料
    //if (ADMIN == "Y") { // 針對維護主管
    //    Ext.getCmp('btnT1Reject').setDisabled(false);
    //    Ext.getCmp('upload').enable();
    //    Ext.getCmp('excel').enable();
    //}
    //else {
    //    Ext.getCmp('btnT1Reject').setDisabled(true);
    //    Ext.getCmp('upload').disable();
    //    Ext.getCmp('excel').disable();
    //}

});
