Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var reportUrl = '/Report/B/BH0002.aspx';
    var T1GetExcel_1 = '../../../api/BH0002/Excel_1';
    var T2GetExcel_2 = '../../../api/BH0002/Excel_2';

    var viewModel = Ext.create('WEBAPP.store.BH.BH0002VM');

    var T1Set = '';
    var T1LastRec = null;
    var T2LastRec = null;

    var PoStore = viewModel.getStore('PO_TSGH');
    var T1Store = viewModel.getStore('WB_MM_PO_M_TSGH');

    var T2Store = viewModel.getStore('WB_REPLY');
    var MmcodeStore = viewModel.getStore('MMCODE');

    var AgennoComboGet = '/api/BH0002/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'agen_no',
        id: 'agen_no',
        fieldLabel: '廠商編號',
        fieldCls: 'required',
        allowBlank: false,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: 70,
        width: 300,
        padding: '0 0 0 0',
        listeners: {
            focus: function (field, event, eOpts) {
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            },
            select: function () {
                T1Query.getForm().findField('P1').setValue('');
                PoStore.removeAll();
                PoStore.getProxy().setExtraParam("AGEN_NO", T1Query.getForm().findField('agen_no').getValue());
                PoStore.load();
            }
        }
    });
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;
        Ext.getCmp('btnImport').setDisabled(true);
        Ext.getCmp('btnPrint').setDisabled(false);
        Ext.getCmp('btnPrintList').setDisabled(false);
        Ext.getCmp('btnDownload').setDisabled(false);
        Ext.getCmp('btnSend').setDisabled(false);

        T1Store.getProxy().setExtraParam("PO_NO", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("AGEN_NO", T1Query.getForm().findField('agen_no').getValue());
        T1Tool.moveFirst();
    }

    function T2Load() {  　
        T2Store.getProxy().setExtraParam("TSGH", 'Y');
        T2Store.getProxy().setExtraParam("AGEN_NO", T1Query.getForm().findField('agen_no').getValue());
        if (T1LastRec) {
            T2Store.getProxy().setExtraParam("PO_NO", T1LastRec.data["PO_NO"]);
            T2Store.getProxy().setExtraParam("MMCODE", T1LastRec.data["MMCODE"]);
            T2Store.getProxy().setExtraParam("DNO", T2Query.getForm().findField('P2').getValue());
            MmcodeStore.getProxy().setExtraParam("PO_NO", T1LastRec.data["PO_NO"]);
        } else {
            T2Store.getProxy().setExtraParam("PO_NO", T1Query.getForm().findField("P1").getValue());
            T2Store.getProxy().setExtraParam("MMCODE", "");
            T2Store.getProxy().setExtraParam("DNO", T2Query.getForm().findField('P2').getValue());
            MmcodeStore.getProxy().setExtraParam("PO_NO", T1Query.getForm().findField("P1").getValue());
        }
        MmcodeStore.load();

        T2Tool.moveFirst();
    }
    function chkMed(mmcode) {
        var ret = "N";
        if (mmcode != null && mmcode.length > 2 && "003,004,005,006,007,009".indexOf(mmcode.substring(0, 3)) > -1) ret = "Y";
        return ret;
    }
    // 查詢欄位
    var mLabelWidth = 90;
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
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        padding: '0 0 5 0',
                        items: [
                            T1FormAgenno,
                            {
                                xtype: 'combo',
                                fieldLabel: '訂單編號',
                                store: PoStore,
                                name: 'P1',
                                id: 'P1',
                                displayField: 'PO_NO',
                                valueField: 'PO_NO',
                                labelWidth: 60,
                                width: 230,
                                padding: '0 4 0 4',
                                listeners: {
                                    change: function () {
                                        T1Store.removeAll();
                                        T2Store.removeAll();
                                        T1LastRec = null;
                                        T2LastRec = null;
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                text: '查詢',
                                handler: function () {
                                    if (T1Query.getForm().findField("P1").getValue() == null)
                                        Ext.Msg.alert('提醒', '輸入訂單編號');
                                    else {
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                        Ext.getCmp('btnDeliveryAdd').setDisabled(false);
                                        msglabel('訊息區:');
                                    }
                                }
                            },
                        ]
                    },
                    {
                        xtype: 'component',
                        autoEl: {
                            tag: 'span',
                            style: 'margin-left: 10px;color:#194c80',
                            html: '※ 請貴公司惠予於交貨前一天，回覆每筆訂單交貨訊息，以利入庫接收時，可迅速清點及接收，感謝貴公司配合。'
                        }
                    }

                ]
            }
        ]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'export', text: '匯出訂單資料', disabled: false,
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: T1Query.getForm().findField("P1").getValue() + '三總訂單.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField("P1").getValue() });
                    p.push({ name: 'TSGH', value: 'Y' });
                    p.push({ name: 'AGEN_NO', value: T1Query.getForm().findField('agen_no').getValue() });
                    PostForm(T1GetExcel_1, p);
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 130,
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 180
            },
            {
                text: "單位",
                dataIndex: 'M_PURUN',
                width: 40,
            },
            {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 90,
                align: 'right'
            },
            {
                text: "訂單數量",
                dataIndex: 'PO_QTY',
                width: 100,
                align: 'right'
            },
            {
                text: "金額",
                dataIndex: 'PO_AMT',
                width: 100,
                align: 'right'
            },
            {
                text: "折讓百分比",
                dataIndex: 'M_DISCPERC',
                width: 100,
                align: 'right'
            },
            {
                text: "廠牌",
                dataIndex: 'M_AGENLAB',
                width: 130,
            },
            {
                header: "",
                flex: 1
            }
        ]
        ,
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');

                T1LastRec = record;
                Ext.getCmp('P2').setValue('');
                Ext.getCmp('btnDeliveryAdd').setDisabled(false);
                Ext.getCmp('btnSearch').setDisabled(false);
                //Ext.getCmp('btnImport').setDisabled(false);
                Ext.getCmp('btnSend').setDisabled(false);
                Ext.getCmp('btnPrint').setDisabled(false);
                Ext.getCmp('btnPrintList').setDisabled(false);
                Ext.getCmp('btnDownload').setDisabled(false);

                T2Load();
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                fieldLabel: 'PO_NO',
                name: 'PO_NO',
                xtype: 'hidden'
            },
            {
                fieldLabel: 'SEQ',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                fieldLabel: '交貨批次',
                name: 'DNO',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                enforceMaxLength: true,
                maxLength: 21,
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'datefield',
                fieldLabel: '預計交貨日',
                name: 'DELI_DT',
                //id: 'D0',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'combo',
                fieldLabel: '院內碼',
                name: 'MMCODE',
                store: MmcodeStore,
                displayField: 'MMCODE',
                valueField: 'MMCODE',
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                enforceMaxLength: true,
                maxLength: 13,
                submitValue: true,
                readOnly: true
            },
            {
                fieldLabel: '交貨數量',
                name: 'INQTY',
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true,
                listeners: {
                    change: function (field, newVal, oldVal) {

                        if (newVal == 0 && newVal != "") {
                            T1Form.getForm().findField('INVOICE_OLD').setVisible(true);
                            T1Form.getForm().findField('INVOICE_OLD').setReadOnly(false);
                            Ext.Msg.alert('訊息提示', '[交貨數量]=0，如有舊發票資料要輸入<font color="red">[舊發票號碼]</font>');
                        }
                        else
                            T1Form.getForm().findField('INVOICE_OLD').setVisible(false);
                    }
                }
            },
            //{
            //    fieldLabel: '借貨量',
            //    name: 'BW_SQTY',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    submitValue: true,
            //    readOnly: true
            //},
            {
                fieldLabel: '批號',
                name: 'LOT_NO',
                enforceMaxLength: true,
                maxLength: 20,
                submitValue: true,
                readOnly: true,
                validator: function (value) {
                    if (T1Form.getForm().findField('INQTY').getValue() != 0 && value != null && value == '') {
                        return '[批號]不可空白';
                    }
                    return true;
                }
            },
            {
                xtype: 'datefield',
                fieldLabel: '效期',
                name: 'EXP_DATE',
                regex: /\d{7,7}/,
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true,
                validator: function (value) {
                    if (T1Form.getForm().findField('INQTY').getValue() != 0 && value != null && value == '') {
                        return '[效期]不可空白';
                    }
                    return true;
                }
            },
            {
                fieldLabel: '<font color="red">舊發票號碼</font>',
                name: 'INVOICE_OLD',
                enforceMaxLength: true,
                maxLength: 10,
                submitValue: true,
                regex: /^[A-Z]{2}[0-9]{8}$/,
                regexText: '請輸入正確發票號碼',
                listeners: {
                    blur: function () {
                        T1Form.getForm().findField('INVOICE_OLD').setValue(Ext.util.Format.uppercase(this.value));
                    }
                }
            },
            {
                fieldLabel: '發票號碼',
                name: 'INVOICE',
                enforceMaxLength: true,
                maxLength: 10,
                submitValue: true,
                readOnly: true,
                regex: /^[A-Z]{2}[0-9]{8}$/,
                regexText: '請輸入正確發票號碼',
                listeners: {
                    blur: function () {
                        T1Form.getForm().findField('INVOICE').setValue(Ext.util.Format.uppercase(this.value));
                    }
                }
            },
            {
                xtype: 'datefield',
                fieldLabel: '發票日期',
                name: 'INVOICE_DT',
                //id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                submitValue: true,
                readOnly: true
            },
            //{
            //    fieldLabel: '條碼編號',
            //    name: 'BARCODE',
            //    enforceMaxLength: true,
            //    maxLength: 50,
            //    submitValue: true,
            //    readOnly: true
            //},
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 100,
                submitValue: true,
                readOnly: true
            },
            {
                xtype: 'textfield',
                name: 'WEXP_ID', // Y=效期管制  
                id: 'WEXP_ID',
                hidden: true
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    var f = T1Form.getForm();
                    var msg = "";
                    var WEXP_ID = f.findField("WEXP_ID").getValue();
                    var LOT_NO = f.findField("LOT_NO").getValue();
                    var EXP_DATE = f.findField("EXP_DATE").getValue();
                    var INVOICE = f.findField("INVOICE").getValue();
                    var INVOICE_DT = f.findField("INVOICE_DT").getValue();
                    var INQTY = f.findField("INQTY").getValue();

                    if (INQTY != 0 && (LOT_NO == "" || EXP_DATE == null))
                        msg += "[效期],[批號]不可以空白";
                    if (WEXP_ID == "Y" && (LOT_NO == "" || EXP_DATE == null))
                        msg += "效期管制-->[效期],[批號]不可以空白";
                    if (INVOICE != "" && INVOICE_DT == null)
                        msg += "[發票日期]不可以空白";
                    if (INVOICE == "" && INVOICE_DT != null)
                        msg += "[發票號碼]不可以空白";
                    if (msg != "") {
                        Ext.Msg.alert('訊息提示', msg);
                    } else {
                        if (this.up('form').getForm().isValid()) {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                    T1Set = '';
                                }
                            });
                        }
                        else
                            Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    }
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                params: {
                    TSGH: 'Y',
                    AGEN_NO: T1Query.getForm().findField('agen_no').getValue()
                },
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            break;
                        case "R":
                            T2Load();
                            msglabel('訊息區:資料退回成功');
                            break;
                    }

                    T1Cleanup();
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
        T1Set = '';
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
    }

    var T2Query = Ext.widget({
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
        items: [
            {
                xtype: 'container',
                layout: 'vbox',
                items: [
                    {
                        xtype: 'panel',
                        border: false,
                        layout: 'hbox',
                        padding: '0 0 5 0',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '交貨批次',
                                name: 'P2',
                                id: 'P2',
                                labelWidth: 60,
                                width: 230,
                                padding: '0 4 0 4',
                                listeners: {
                                    change: function () {
                                        Ext.getCmp('btnDeliveryAdd').setDisabled(false);
                                        Ext.getCmp('btnSearch').setDisabled(false);
                                        Ext.getCmp('btnPrint').setDisabled(false);
                                        Ext.getCmp('btnPrintList').setDisabled(false);
                                        Ext.getCmp('btnDownload').setDisabled(false);
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnSearch',
                                text: '查詢',
                                handler: function () {
                                    T2Grid.columns[1].setVisible(false);
                                    T2Grid.columns[2].setVisible(false);
                                    T2Grid.getSelectionModel().deselectAll(true);
                                    T2Load();
                                    msglabel('訊息區:');
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnDeliveryAdd',
                                text: '交貨新增',
                                handler: function () {
                                    Ext.MessageBox.confirm('訊息', '是否確定新增交貨批次?', function (btn, text) {
                                        if (btn === 'yes') {
                                            Ext.Ajax.request({
                                                url: '/api/BH0002/CreateAll',
                                                method: reqVal_p,
                                                params: {
                                                    PO: T1Query.getForm().findField("P1").getValue(),
                                                    TSGH: 'Y',
                                                    AGEN_NO: T1Query.getForm().findField('agen_no').getValue()
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    if (data.success) {
                                                        msglabel('訊息區:新增交貨批次成功');
                                                        T2Grid.getSelectionModel().deselectAll(true);
                                                        T2Load();
                                                    }
                                                },
                                                failure: function (response) {
                                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                }
                                            });
                                        }
                                    });
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnAdd',
                                text: '新增',
                                handler: function () {
                                    T1Set = '/api/BH0002/CreateOne';
                                    setFormT1('I', '新增');
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnUpdate',
                                text: '修改',
                                handler: function () {
                                    T1Set = '/api/BH0002/DetailUpdate';
                                    setFormT1('U', '修改');
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnDelete',
                                text: '刪除',
                                handler: function () {
                                    var selection = T2Grid.getSelection();
                                    if (selection.length) {
                                        let seq = '';
                                        //selection.map(item => {
                                        //    seq += item.get('SEQ') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            seq += item.get('SEQ') + ',';
                                        })
                                        Ext.MessageBox.confirm('刪除', '是否確定刪除?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/BH0002/DetailDelete',
                                                    method: reqVal_p,
                                                    params: {
                                                        SEQ: seq
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            msglabel('訊息區:刪除成功');
                                                            T2Grid.getSelectionModel().deselectAll(true);
                                                            T2Load();
                                                        }
                                                        else {
                                                            Ext.MessageBox.alert('錯誤', data.msg);
                                                            msglabel('訊息區:' + data.msg);
                                                        }
                                                    },
                                                    failure: function (response) {
                                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                    }
                                                });
                                            }
                                        });
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnSubmit',
                                text: '確認回傳',
                                handler: function () {
                                    var selection = T2Grid.getSelection();
                                    if (selection.length) {
                                        let seq = '';
                                        let inqty = '';
                                        let mmcode = '';
                                        let po_no = '';
                                        //selection.map(item => {
                                        //    seq += item.get('SEQ') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            seq += item.get('SEQ') + ',';
                                            inqty += item.get('INQTY') + ',';
                                            mmcode += item.get('MMCODE') + ',';
                                            po_no += item.get('PO_NO') + ',';
                                        })
                                        Ext.MessageBox.confirm('訊息', '是否確定回傳?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/BH0002/DetailSubmit',
                                                    method: reqVal_p,
                                                    params: {
                                                        SEQ: seq,
                                                        INQTY: inqty,
                                                        MMCODE: mmcode,
                                                        PO_NO: po_no
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            if (data.msg.toString() == "") {
                                                                msglabel('訊息區:確認回傳成功');
                                                                T2Grid.getSelectionModel().deselectAll(true);
                                                                T2Load();
                                                            } else {
                                                                Ext.MessageBox.alert('警示', data.msg);
                                                            }
                                                        }
                                                        else {
                                                            Ext.MessageBox.alert('錯誤', data.msg);
                                                            msglabel('訊息區:' + data.msg);
                                                        }
                                                    },
                                                    failure: function (response) {
                                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                    }
                                                });
                                            }
                                        });
                                    }
                                }
                            },
                            {
                                xtype: 'filefield',
                                name: 'send',
                                id: 'btnSend',
                                disabled: true,
                                buttonOnly: true,
                                buttonText: '選擇檔案匯入',
                                width: 90,
                                listeners: {
                                    change: function (widget, value, eOpts) {
                                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                        Ext.getCmp('btnImport').setDisabled(true);
                                        T1Store.removeAll();
                                        T2Grid.columns[1].setVisible(false);
                                        T2Grid.columns[2].setVisible(false);
                                        var files = event.target.files
                                        var self = this; // the controller
                                        if (!files || files.length == 0) return; // make sure we got something
                                        var f = files[0];
                                        var ext = this.value.split('.').pop();
                                        if (!/^(xls|xlsx)$/.test(ext)) {
                                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                                            T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                            msglabel("訊息區:");
                                        }
                                        else {
                                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                            myMask.show();
                                            var formData = new FormData();
                                            formData.append("file", f);
                                            formData.append("po_no", T1Query.getForm().findField("P1").getValue());
                                            formData.append("TSGH", "Y");
                                            formData.append("AGEN_NO", T1Query.getForm().findField('agen_no').getValue());
                                            var ajaxRequest = $.ajax({
                                                type: "POST",
                                                url: "/api/BH0002/SendExcel",
                                                data: formData,
                                                processData: false,
                                                //必須false才會自動加上正確的Content-Type
                                                contentType: false,
                                                success: function (data, textStatus, jqXHR) {
                                                    if (!data.success) {
                                                        T2Store.removeAll();
                                                        Ext.MessageBox.alert("提示", data.msg);
                                                        msglabel("訊息區:");
                                                        Ext.getCmp('btnImport').setDisabled(true);
                                                        IsSend = false;
                                                    }
                                                    else {
                                                        msglabel("訊息區:檔案讀取成功");
                                                        T2Store.loadData(data.etts, false);
                                                        IsSend = true;
                                                        Ext.getCmp('btnImport').setDisabled(false);
                                                        T2Grid.columns[1].setVisible(true);
                                                        T2Grid.columns[2].setVisible(true);
                                                    }
                                                    T2Query.getForm().findField('send').fileInputEl.dom.value = '';
                                                    myMask.hide();

                                                },
                                                error: function (jqXHR, textStatus, errorThrown) {
                                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                                    T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                                    Ext.getCmp('btnImport').setDisabled(true);
                                                    myMask.hide();

                                                }
                                            });
                                        }
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnImport',
                                text: '匯入',
                                handler: function () {
                                    Ext.MessageBox.confirm('提示', '<font color="red">檢查不合格資料不會匯入</font>，是否確定匯入?', function (btn, text) {
                                        if (btn === 'yes') {
                                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                            myMask.show();
                                            Ext.Ajax.request({
                                                url: '/api/BH0002/Import',
                                                method: reqVal_p,
                                                params: {
                                                    data: Ext.encode(Ext.pluck(T2Store.data.items, 'data')),
                                                    PO_NO: T1Query.getForm().findField("P1").getValue()
                                                },
                                                success: function (response) {
                                                    var data = Ext.decode(response.responseText);
                                                    if (data.success) {
                                                        T2Store.loadData(data.etts, false);
                                                        T2Query.getForm().findField("P2").setValue(data.msg);
                                                        Ext.getCmp('btnSearch').setDisabled(false);
                                                        Ext.MessageBox.alert("提示", "匯入完成，[交貨批次]=" + data.msg);
                                                        T2Grid.columns[1].setVisible(false);
                                                        T2Grid.columns[2].setVisible(false);
                                                        T2Grid.getSelectionModel().deselectAll(true);
                                                        T2Load();
                                                        msglabel("訊息區:匯入完成");
                                                        Ext.getCmp('btnImport').setDisabled(true);
                                                    }
                                                    myMask.hide();
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
                                                            Ext.Msg.alert('失敗', "匯入失敗");
                                                            break;
                                                    }
                                                }
                                            });
                                        }
                                    });
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnPrint',
                                text: '列印',
                                handler: function () {
                                    if (T2Query.getForm().findField('P2').getValue() == "" || T2Query.getForm().findField('P2').getValue() == null) {
                                        Ext.Msg.alert('訊息', '需填交貨批次才能列印');
                                    }
                                    else {
                                        showReport(false);
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnPrintList',
                                text: '列印清單',
                                handler: function () {
                                    if (T2Query.getForm().findField('P2').getValue() == "" || T2Query.getForm().findField('P2').getValue() == null) {
                                        Ext.Msg.alert('訊息', '需填交貨批次才能列印');
                                    }
                                    else {
                                        showReport(true);
                                    }
                                }
                            },
                            {
                                xtype: 'button',
                                id: 'btnDownload',
                                text: '匯出範本',
                                handler: function () {
                                    var p = new Array();
                                    p.push({ name: 'FN', value: T1Query.getForm().findField("P1").getValue() + '三總送貨單.xls' });
                                    p.push({ name: 'p0', value: T1Query.getForm().findField("P1").getValue() });
                                    p.push({ name: 'p2', value: P2 });
                                    p.push({ name: 'TSGH', value: 'Y' });
                                    p.push({ name: 'AGEN_NO', value: T1Query.getForm().findField('agen_no').getValue() });
                                    PostForm(T2GetExcel_2, p);
                                    msglabel('訊息區:匯出完成');
                                }
                            },
                        ]
                    },
                    {
                        xtype: 'component',
                        autoEl: {
                            tag: 'span',
                            style: 'margin-left: 10px;color:#194c80',
                            html: '※ [交貨新增] 會複製訂單所有品項資料，再填上相關欄位。<br>※ 條碼編號係指藥品出貨之最小單位包裝之條碼。<br>※ <font color="red">同院內碼不同批號丶效期，請[新增]輸入成另一筆資料。</font><br>※ <font color="red">更新發票資料請新增一筆資料，[交貨數量]填"0"，<font color="blue">如有舊發票資料要輸入[舊發票號碼]，沒有不用輸入</font>。</font>'
                        }
                    }
                ]
            }
        ]
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T2Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "檢核結果",
                dataIndex: 'CHECK_RESULT',
                width: 150,
                hidden: true
            }, {
                text: "異動結果",
                dataIndex: 'IMPORT_RESULT',
                width: 120,
                hidden: true
            },
            {
                text: "交貨批次",
                dataIndex: 'DNO',
                width: 80
            },
            {
                text: "預計交貨日",
                dataIndex: 'DELI_DT',
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "交貨數量",
                dataIndex: 'INQTY',
                width: 80
            },
            //{
            //    text: "借貨量",
            //    dataIndex: 'BW_SQTY',
            //    width: 80,
            //},
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 150,
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 100,
            },
            {
                text: "發票號碼",
                dataIndex: 'INVOICE',
                width: 100,
            },
            {
                text: "發票日期",
                dataIndex: 'INVOICE_DT',
                width: 100,
            },
            {
                text: "舊發票號碼",
                dataIndex: 'INVOICE_OLD',
                hidden: true,
                width: 100,
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 100,
            },
            {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 100,
            },
            {
                xtype: 'hidden',
                dataIndex: 'SEQ'
            },
            {
                xtype: 'hidden',
                dataIndex: 'PO_NO'
            },
            {
                xtype: 'hidden',
                dataIndex: 'BARCODE'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden == true) {
                        T1Form.setVisible(true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {
                        Ext.getCmp('btnAdd').setDisabled(false);
                        //Ext.getCmp('btnSubmit').setDisabled(false);

                        // STATUS是A(處理中)
                        if (T2LastRec.data['STATUS'] == '處理中') {
                            Ext.getCmp('btnUpdate').setDisabled(false);
                            //Ext.getCmp('btnDelete').setDisabled(false);
                            Ext.getCmp('btnSubmit').setDisabled(false);
                        }
                        else {
                            Ext.getCmp('btnUpdate').setDisabled(true);
                            //Ext.getCmp('btnDelete').setDisabled(true);
                            Ext.getCmp('btnSubmit').setDisabled(true);
                        }

                        if (T1Set === '')
                            setFormT1a();

                        //T2Load();
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T2LastRec = records[0];
                setFormT1a();
                if (T2LastRec == null) {
                    Ext.getCmp('btnAdd').setDisabled(true);
                    Ext.getCmp('btnUpdate').setDisabled(true);
                    //Ext.getCmp('btnDelete').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                }
                else {
                    Ext.getCmp('btnAdd').setDisabled(false);
                    Ext.getCmp('btnUpdate').setDisabled(false);
                    Ext.getCmp('btnSubmit').setDisabled(false);
                }
            }
        }
    });

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.WB_REPLY'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField('DNO').setValue(T1LastRec.data['DNO']);
            f.findField('MMCODE').setValue(T1LastRec.data['MMCODE']);
            f.findField('PO_NO').setValue(T1LastRec.data['PO_NO']);
            //f.findField("BW_SQTY").setValue('0');
        }
        else {

        }
        f.findField('x').setValue(x);
        f.findField('DELI_DT').setReadOnly(false);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('INQTY').setReadOnly(false);
        //f.findField('BW_SQTY').setReadOnly(false);
        f.findField('INVOICE').setReadOnly(false);
        f.findField('INVOICE_DT').setReadOnly(false);
        //f.findField('BARCODE').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        //u.focus();
    }

    function setFormT1a() {
        if (T2LastRec != null) {
            viewport.down('#form').expand();
            viewport.down('#form').setTitle('瀏覽');

            //var f = T1Form.getForm();
            T1Form.loadRecord(T2LastRec);
            //f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            //f.findField("DNO").setValue(T2LastRec.data["DNO"]);
            //f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            //f.findField("DELI_DT").setValue(T2LastRec.data["DELI_DT"]);
            //f.findField("LOT_NO").setValue(T2LastRec.data["LOT_NO"]);
            //f.findField("EXP_DATE").setValue(T2LastRec.data["EXP_DATE"]);

            //f.findField("INQTY").setValue(T2LastRec.data["INQTY"]);
            ////f.findField("BW_SQTY").setValue(T2LastRec.data["BW_SQTY"]);
            //f.findField("INVOICE").setValue(T2LastRec.data["INVOICE"]);
            //f.findField("INVOICE_OLD").setValue(T2LastRec.data["INVOICE_OLD"]);
            //f.findField("INVOICE_DT").setValue(T2LastRec.data["INVOICE_DT"]);
            ////f.findField("BARCODE").setValue(T2LastRec.data["BARCODE"]);
            //f.findField("MEMO").setValue(T2LastRec.data["MEMO"]);
            //f.findField("WEXP_ID").setValue(T2LastRec.data["WEXP_ID"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    function showReport(IsList) {
        if (!win) {
            var np = {
                //p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T2Query.getForm().findField('P2').getValue(),
                p3: IsList,
                TSGH: 'Y',
                AGEN_NO: T1Query.getForm().findField('agen_no').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '&TSGH=' + np.TSGH + '&AGEN_NO=' + np.AGEN_NO + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
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

    Ext.getCmp('btnAdd').setDisabled(true);
    Ext.getCmp('btnUpdate').setDisabled(true);
    //Ext.getCmp('btnDelete').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);

    Ext.getCmp('btnDeliveryAdd').setDisabled(true);
    Ext.getCmp('btnSearch').setDisabled(true);
    Ext.getCmp('btnImport').setDisabled(true);
    Ext.getCmp('btnPrint').setDisabled(true);
    Ext.getCmp('btnPrintList').setDisabled(true);
    Ext.getCmp('btnDownload').setDisabled(true);
});