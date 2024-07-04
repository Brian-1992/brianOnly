Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.tip.QuickTipManager.init();
    // var T1Get = '/api/BE0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "發票管理維護";

    var T1Rec = 0;
    var T1LastRec = null;
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var ADMIN = "N";
    if (Ext.getUrlParam('ADMIN') != null) ADMIN = Ext.getUrlParam('ADMIN');
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var st_contid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "0", "TEXT": "合約" },
            { "VALUE": "2", "TEXT": "非合約" },
            { "VALUE": "3", "TEXT": "小採" }
        ]
    });
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0003/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, records, successful) {
                if (store.data.length > 0) {
                    T1Query.getForm().findField('P0').setValue(store.data.items[0].data.VALUE);
                    if (store.data.items[0].data.VALUE == '01') //藥品, 重新設定[合約類別]st_contid
                    {
                        Ext.getCmp('btnT1back').setVisible(false);
                        st_contid.removeAll();
                        st_contid.add({ "VALUE": "0", "TEXT": "合約" });
                        st_contid.add({ "VALUE": "2", "TEXT": "零購" });
                    }
                }
            }
        }
    });

    var st_invoiceexist = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0003/GetInvoiceExistCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_ckstatus = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "N", "TEXT": "未驗證" },
            { "VALUE": "Y", "TEXT": "已驗證" },
            { "VALUE": "T", "TEXT": "驗退" }
        ]
    });
    var st_memo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0003/GetMemoCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //檢核廠商數
    function chkVENDER() {
        var AGEN_NO_TEMP = '';
        var CHECK_FLAG = 'Y';
        if (T1Grid.getSelection().length > 1) { AGEN_NO_TEMP = T1Grid.getSelection()[0].get('AGEN_NO') };
        for (var i = 1; i < T1Grid.getSelection().length; i++) {
            if (T1Grid.getSelection()[i].get('AGEN_NO') != AGEN_NO_TEMP) {
                CHECK_FLAG = 'N';
            }
        }
        return CHECK_FLAG;
    }
    //檢核發票日期
    function chkINVOICE(invoice, dt) {
        var CHECK_FLAG = 'Y';
        for (var i = 1; i < T1Grid.store.data.length; i++) {
            if (T1Grid.store.data.items[i].get('INVOICE') == invoice && T1Grid.store.data.items[i].get('INVOICE_DT') != dt) {
                CHECK_FLAG = 'N';
            }
        }
        return CHECK_FLAG;
    }
    // 查詢欄位
    var mLabelWidth = 80;
    var mWidth = 200;
    var AgennoComboGet = '/api/BE0003/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo_1', {
        name: 'P4',
        id: 'P4',
        fieldLabel: '廠商編號',
        fieldCls: 'required',
        allowBlank: false,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: mLabelWidth,
        width: 260,
        //margin: '1 0 1 0',
        padding: '0 1 0 1',
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P4').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth,
            margin: '2 4 0 0'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: ' 物料分類',
                    name: 'P0',
                    id: 'P0',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true,
                    allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 420,
                    padding: '0 1 0 1'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '訂單日期',
                    name: 'P1',
                    id: 'P1',
                    vtype: 'dateRange',
                    dateRange: { end: 'P2' },
                    padding: '0 1 0 1',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: mWidth,
                    value: getFirstday()
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    name: 'P2',
                    id: 'P2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'P1' },
                    padding: '0 1 0 1',
                    //allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    labelAlign: 'right',
                    labelWidth: 10,
                    width: 130,
                    value: getToday()
                }, {
                    xtype: 'combo',
                    fieldLabel: '合約類別',
                    name: 'P6',
                    id: 'P6',
                    store: st_contid,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    labelAlign: 'right',
                    labelWidth: mLabelWidth,
                    width: 150,
                    padding: '0 1 0 1'
                }
            ]
        }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    T1FormAgenno, {
                        xtype: 'textfield',
                        fieldLabel: '訂單號碼',
                        name: 'P8',
                        id: 'P8',
                        enforceMaxLength: true,
                        width: 200
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '發票號碼',
                        name: 'P5',
                        id: 'P5',
                        enforceMaxLength: true,
                        maxLength: 10,
                        width: 180
                    }, {
                        xtype: 'combo',
                        fieldLabel: '發票維護否',
                        name: 'P3',
                        id: 'P3',
                        store: st_invoiceexist,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: 180
                    }, {
                        xtype: 'combo',
                        fieldLabel: '驗證狀態',
                        name: 'P7',
                        id: 'P7',
                        store: st_ckstatus,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: 140,
                        value: ''
                    }
                ]
            }, {
                xtype: 'panel',
                border: false,
                layout: 'hbox',
                items: [
                    , {
                        xtype: 'datefield',
                        fieldLabel: '進貨日期',
                        name: 'P9',
                        id: 'P9',
                        vtype: 'dateRange',
                        dateRange: { end: 'P10' },
                        padding: '0 1 0 1',
                        allowBlank: true,
                        labelAlign: 'right',
                        labelWidth: mLabelWidth,
                        width: mWidth
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P10',
                        id: 'P10',
                        labelSeparator: '',
                        vtype: 'dateRange',
                        dateRange: { begin: 'P9' },
                        padding: '0 1 0 1',
                        allowBlank: true,
                        labelAlign: 'right',
                        labelWidth: 10,
                        width: 130
                    }
                ]
            }, {
            xtype: 'panel',
            id: 'PanelP3',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '設定發票號碼',
                    name: 'I_INVOICE',
                    id: 'I_INVOICE',
                    enforceMaxLength: true,
                    maxLength: 10,
                    margin: '2 4 0 0',
                    regex: /[A-Z0-9]/,
                    regexText: '請輸入正確發票號碼',
                    listeners: {
                        blur: function () {
                            T1Query.getForm().findField('I_INVOICE').setValue(Ext.util.Format.uppercase(this.value));
                        }
                    }
                    //}, {
                    //    xtype: 'button',
                    //    text: '設定發票',
                    //    name: 'btnT1Invoice', id: 'btnT1Invoice', disabled: true,
                    //    handler: function () {
                    //        if (this.up('form').getForm().findField('I_INVOICE').getValue() == '' || this.up('form').getForm().findField('I_INVOICE').getValue() == null) 
                    //            Ext.Msg.alert('提醒', '設定發票號碼不可空白');
                    //        else {
                    //            var selection = T1Grid.getSelection();
                    //            var i_invoice = this.up('form').getForm().findField('I_INVOICE').getValue();
                    //            if (selection.length) {
                    //                let name = '';
                    //                let po_no = '';
                    //                let mmcode = '';
                    //                let transno = '';
                    //                selection.map(item => {
                    //                    name += '「' + item.get('PO_NO') + '」<br>';
                    //                    po_no += item.get('PO_NO') + ',';
                    //                    mmcode += item.get('MMCODE') + ',';
                    //                    transno += item.get('TRANSNO') + ',';
                    //                });

                    //                Ext.MessageBox.confirm('提醒', '是否確定設定發票?', function (btn, text) {
                    //                    if (btn === 'yes') {
                    //                        Ext.Ajax.request({
                    //                            url: '/api/BE0003/UpdateInvoice',
                    //                            method: reqVal_p,
                    //                            params: {
                    //                                PO_NO: po_no,
                    //                                MMCODE: mmcode,
                    //                                TRANSNO: transno,
                    //                                INVOICE: i_invoice
                    //                            },
                    //                            success: function (response) {
                    //                                var data = Ext.decode(response.responseText);
                    //                                if (data.success) {
                    //                                    T1Grid.getSelectionModel().deselectAll();
                    //                                    T1Load();
                    //                                    msglabel('訊息區:設定發票成功');
                    //                                }
                    //                                else
                    //                                    Ext.MessageBox.alert('錯誤', data.msg);
                    //                            },
                    //                            failure: function (response) {
                    //                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    //                            }
                    //                        });
                    //                    }
                    //                });
                    //            }
                    //        }
                    //    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '發票日期',
                    name: 'I_INVOICE_DT',
                    id: 'I_INVOICE_DT'
                }, {
                    xtype: 'button',
                    text: '設定發票/日期',
                    name: 'btnT1Invoicedt', id: 'btnT1Invoicedt', disabled: true,
                    margin: '2 4 0 0',
                    handler: function () {
                        if (this.up('form').getForm().isValid()) {
                            if (this.up('form').getForm().findField('I_INVOICE_DT').getValue() == '' || this.up('form').getForm().findField('I_INVOICE_DT').getValue() == null ||
                                this.up('form').getForm().findField('I_INVOICE').getValue() == '' || this.up('form').getForm().findField('I_INVOICE').getValue() == null)
                                Ext.Msg.alert('提醒', '[設定發票號碼]或[發票日期]不可空白');
                            else {
                                var selection = T1Grid.getSelection();
                                var i_invoiceDT = this.up('form').getForm().findField('I_INVOICE_DT').getValue();
                                var i_invoice = this.up('form').getForm().findField('I_INVOICE').getValue();
                                if (chkVENDER() == 'Y') {
                                    //if (chkINVOICE(i_invoice, this.up('form').getForm().findField('I_INVOICE_DT').rawValue) == 'Y') {
                                    if (selection.length) {
                                        let name = '';
                                        let po_no = '';
                                        let mmcode = '';
                                        let transno = '';
                                        //selection.map(item => {
                                        //    name += '「' + item.get('PO_NO') + '」<br>';
                                        //    po_no += item.get('PO_NO') + ',';
                                        //    mmcode += item.get('MMCODE') + ',';
                                        //    transno += item.get('TRANSNO') + ',';
                                        //});
                                        $.map(selection, function (item, key) {
                                            name += '「' + item.get('PO_NO') + '」<br>';
                                            po_no += item.get('PO_NO') + ',';
                                            mmcode += item.get('MMCODE') + ',';
                                            transno += item.get('TRANSNO') + ',';
                                        })
                                        Ext.MessageBox.confirm('提醒', '確定設定[發票號碼]及[發票日期]?', function (btn, text) {
                                            if (btn === 'yes') {
                                                Ext.Ajax.request({
                                                    url: '/api/BE0003/UpdateInvoicedt',
                                                    method: reqVal_p,
                                                    params: {
                                                        PO_NO: po_no,
                                                        MMCODE: mmcode,
                                                        TRANSNO: transno,
                                                        INVOICE_DT: i_invoiceDT,
                                                        INVOICE: i_invoice
                                                    },
                                                    success: function (response) {
                                                        var data = Ext.decode(response.responseText);
                                                        if (data.success) {
                                                            if (data.msg.toString() == "") {
                                                                T1Grid.getSelectionModel().deselectAll();
                                                                T1Load();
                                                                msglabel('訊息區:設定[發票號碼]及[發票日期]成功');
                                                            } else {
                                                                Ext.MessageBox.alert('警示', data.msg);
                                                            }
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
                                    //}
                                    //else
                                    //{
                                    //    Ext.MessageBox.alert('提醒', '[發票號碼]存在不同[發票日期],請更正');
                                    //}
                                }
                                else
                                { Ext.Msg.alert('提醒', '選擇超過1家以上廠商,請更正'); }
                            }
                        } else {
                            Ext.Msg.alert('提醒', '[設定發票號碼]或[發票日期]格式有誤,請更正');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '查詢',
                    margin: '2 4 0 20',
                    handler: function () {
                        if (
                            (this.up('form').getForm().findField('P0').getValue() == '' || this.up('form').getForm().findField('P0').getValue() == null) ||
                            (this.up('form').getForm().findField('P1').getValue() == '' || this.up('form').getForm().findField('P1').getValue() == null) ||
                            (this.up('form').getForm().findField('P2').getValue() == '' || this.up('form').getForm().findField('P2').getValue() == null) ||
                            (this.up('form').getForm().findField('P4').getValue() == '' || this.up('form').getForm().findField('P4').getValue() == null)
                        ) {
                            Ext.Msg.alert('提醒', '[物料分類]、[訂單日期]及[廠商編號]不可空白');
                        }
                        else {
                            T1Load();
                            msglabel('訊息區:');
                            btnControl('2');
                        }
                    }
                    //}, {
                    //    xtype: 'button',
                    //    text: '清除',
                    //    margin: '2 4 0 0',
                    //    handler: function () {
                    //        var f = this.up('form').getForm();
                    //        f.reset();
                    //        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    //        msglabel('訊息區:');
                    //        btnControl('2');
                    //    }
                }, {
                    xtype: 'button', text: '修改',
                    name: 'btnT1edit', id: 'btnT1edit',
                    disabled: true,
                    margin: '2 4 0 0',
                    handler: function () {
                        T1Set = '/api/BE0003/UpdateM';
                        msglabel('訊息區:');
                        if (T1LastRec) {
                            setFormT1("U", '修改');
                        }
                        else {
                            btnControl('2'); T1Cleanup(); viewport.down('#form').collapse();
                        }
                    }
                }, {
                    xtype: 'button', text: '驗退',
                    name: 'btnT1back', id: 'btnT1back',
                    disabled: true,
                    margin: '2 4 0 0',
                    handler: function () {
                        showWin2();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button', text: '驗證完成',
                    name: 'btnT1apply', id: 'btnT1apply',
                    disabled: true,
                    margin: '2 4 0 0',
                    handler: function () {
                        if (T1Form.getForm().findField('INVOICE').getValue() == '' || T1Form.getForm().findField('INVOICE_DT').getValue() == '') {
                            Ext.Msg.alert('提醒', '請輸入發票號碼及發票日期');
                        }
                        else {
                            var selection = T1Grid.getSelection();
                            if (selection.length) {
                                let name = '';
                                let po_no = '';
                                let mmcode = '';
                                let transno = '';
                                //selection.map(item => {
                                //    name += '「' + item.get('PO_NO') + '」<br>';
                                //    po_no += item.get('PO_NO') + ',';
                                //    mmcode += item.get('MMCODE') + ',';
                                //    transno += item.get('TRANSNO') + ',';
                                //});
                                $.map(selection, function (item, key) {
                                    name += '「' + item.get('PO_NO') + '」<br>';
                                    po_no += item.get('PO_NO') + ',';
                                    mmcode += item.get('MMCODE') + ',';
                                    transno += item.get('TRANSNO') + ',';
                                })

                                Ext.MessageBox.confirm('提醒', '是否確定驗證完成?', function (btn, text) {
                                    if (btn === 'yes') {
                                        Ext.Ajax.request({
                                            url: '/api/BE0003/UpdateA',
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
                                                    msglabel('驗證成功');
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
                    }
                }, {
                    xtype: 'button', text: '刪除',
                    name: 'btnT1Del', id: 'btnT1Del',
                    margin: '2 4 0 0',
                    //handler: function () {
                    //    T1Set = '/api/BE0003/UpdateDEL';
                    //    msglabel('訊息區:');
                    //    setFormT1("D", '刪除');
                    //    if (T1LastRec) { }
                    //    else {
                    //        btnControl('2'); T1Cleanup(); viewport.down('#form').collapse();
                    //    }
                    //}
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
                                        url: '/api/BE0003/UpdateDEL',
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
                    xtype: 'button', text: '新增',
                    name: 'btnT1add', id: 'btnT1add',
                    disabled: true,
                    margin: '2 4 0 0',
                    handler: function () {
                        T1Set = '/api/BE0003/CreateM';
                        msglabel('訊息區:');
                        setFormT1("I", '新增');
                        if (T1LastRec) { }
                        else {
                            btnControl('2'); T1Cleanup(); viewport.down('#form').collapse();
                        }
                    }
                }, {
                    xtype: 'button', text: '取消驗證',
                    name: 'btnT1Reject', id: 'btnT1Reject',
                    disabled: true,
                    margin: '2 4 0 0',
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

                            Ext.MessageBox.confirm('提醒', '是否確定[取消驗證]?', function (btn, text) {
                                if (btn === 'yes') {
                                    Ext.Ajax.request({
                                        url: '/api/BE0003/Reject',
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
                                                msglabel('取消驗證成功');
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
                },
                {
                    xtype: 'filefield',
                    buttonText: '匯入',
                    buttonOnly: true,
                    padding: '2 4 0 0',
                    id: 'upload',
                    width: 72,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            
                            var files = event.target.files;
                            if (!files || files.length == 0) return; // make sure we got something
                            var file = files[0];
                            var ext = this.value.split('.').pop();
                            if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                                Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                                Ext.getCmp('import').fileInputEl.dom.value = '';
                                msglabel('');
                            } else {
                                var myMaskEditWindow = new Ext.LoadMask(Ext.getCmp('uploadWindow'), { msg: '處理中...' });
                                msglabel("已選擇檔案");
                                myMaskEditWindow.show();
                                var formData = new FormData();
                                formData.append("file", file);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/BE0003/UploadCheck",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        myMaskEditWindow.hide();

                                        if (!data.success) {
                                            
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區:");
                                            Ext.getCmp('upload').setRawValue("");
                                            Ext.getCmp('upload').fileInputEl.dom.value = '';
                                        }
                                        else {
                                            Ext.getCmp('upload').setRawValue("");
                                            Ext.getCmp('upload').fileInputEl.dom.value = '';

                                            uploadWindow.show();
                                            Ext.getCmp('uploadConfirm').enable();
                                            T2Store.removeAll();
                                            for (var i = 0; i < data.etts.length; i++) {
                                                T2Store.add(data.etts[i]);
                                                
                                                if (data.etts[i].ITEM_STRING != '') {
                                                    Ext.getCmp('uploadConfirm').disable();
                                                }
                                            }

                                        }
                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        myMaskEditWindow.hide();
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        //T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                        Ext.getCmp('upload').setRawValue("");
                                    }
                                });
                            }
                        }

                    }

                }, {
                    xtype: 'button', text: '匯出',
                    name: 'excel', id: 'excel',
                    disabled: true,
                    margin: '2 0 0 0',
                    handler: function () {
                        
                        if (T1Query.getForm().findField('P0').getValue().length == 0 ||
                            !T1Query.getForm().findField('P1').getValue() ||
                            !T1Query.getForm().findField('P2').getValue()) {
                            Ext.Msg.alert('提醒', '<span style="color:red">物料類別</span>、<span style="color:red">起迄日期</span>必填');
                            return;
                        }

                        var p = new Array();
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').rawValue });
                        p.push({ name: 'p2', value: T1Query.getForm().findField('P2').rawValue });
                        p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                        p.push({ name: 'p9', value: T1Query.getForm().findField('P9').rawValue });
                        p.push({ name: 'p10', value: T1Query.getForm().findField('P10').rawValue });
                        PostForm('/api/BE0003/GetExcel', p);
                    }
                }
            ]
        }]
    });
    function btnControl(id) {
        if (id == '1') {
            Ext.getCmp('btnT1edit').setDisabled(false);
            Ext.getCmp('btnT1add').setDisabled(false);
            Ext.getCmp('btnT1Del').setDisabled(false);
            //Ext.getCmp('btnT1Invoice').setDisabled(false);
            Ext.getCmp('btnT1Invoicedt').setDisabled(false);
            if (T1Query.getForm().findField('P0').getValue == '01') //藥品, 
                Ext.getCmp('btnT1back').setVisible(false);
            else {
                Ext.getCmp('btnT1back').setVisible(true);
                Ext.getCmp('btnT1back').setDisabled(false);
            }
            Ext.getCmp('btnT1apply').setDisabled(false);
        }
        else {
            Ext.getCmp('btnT1edit').setDisabled(true);
            Ext.getCmp('btnT1add').setDisabled(true);
            Ext.getCmp('btnT1Del').setDisabled(true);
            //Ext.getCmp('btnT1Invoice').setDisabled(true);
            Ext.getCmp('btnT1Invoicedt').setDisabled(true);
            if (T1Query.getForm().findField('P0').getValue == '01') //藥品, 
                Ext.getCmp('btnT1back').setVisible(false);
            else {
                Ext.getCmp('btnT1back').setVisible(true);
                Ext.getCmp('btnT1back').setDisabled(true);
            }
            Ext.getCmp('btnT1apply').setDisabled(true);
        }
    }
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    function getFirstday() {
        var date = new Date(), y = date.getFullYear(), m = date.getMonth();
        var firstDay = new Date(y, m, 1);
        var lastDay = new Date(y, m + 1, 0);
        return firstDay
    }
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'PO_NO', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'PR_DEPT', type: 'date' },
            { name: 'PO_QTY', type: 'string' },
            { name: 'PO_PRICE', type: 'string' },
            { name: 'M_PURUN', type: 'string' },
            { name: 'M_AGENLAB', type: 'string' },
            { name: 'PO_AMT', type: 'string' },
            { name: 'M_DISCPERC', type: 'string' },
            { name: 'DELI_QTY', type: 'string' },
            { name: 'BW_QTY', type: 'string' },
            { name: 'DELI_STATUS', type: 'string' },
            { name: 'BW_QTY', type: 'string' },
            { name: 'CREATE_TIME', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_TIME', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MEMO', type: 'string' },
            { name: 'PR_NO', type: 'string' },
            { name: 'UNIT_SWAP', type: 'string' },
            { name: 'INVOICE', type: 'string' },
            { name: 'INVOICE_DT', type: 'string' },
            { name: 'CKIN_QTY', type: 'string' },
            { name: 'CHK_USER', type: 'string' },
            { name: 'CHK_DT', type: 'string' },
            { name: 'AGEN_NO', type: 'string' },
            { name: 'CHG_CNT', type: 'integer' },
            { name: 'CHG_LIST', type: 'string' },
            { name: 'INVOICE_OLD', type: 'string' },
            { name: 'TRANSNO_OLD', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        //sorters: [{ property: 'PO_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0003/All',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').rawValue,
                    p2: T1Query.getForm().findField('P2').rawValue,
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                    p8: T1Query.getForm().findField('P8').getValue(),
                    p9: T1Query.getForm().findField('P9').rawValue,
                    p10: T1Query.getForm().findField('P10').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
        T1Store.load();
        viewport.down('#form').collapse();
    }

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        if (T1LastRec) {
            viewport.down('#form').expand();
        }
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.BE0003');
            //T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("PO_NO");
            f.findField('INVOICE').setValue('');
            f.findField('INVOICE_DT').setValue('');
            f.findField('MEMO').setValue('');
            u.setReadOnly(false);
            u.clearInvalid();
        }
        else {
            u = f.findField('PO_NO');
        }
        f.findField('x').setValue(x);
        f.findField('INVOICE').setReadOnly(false);
        f.findField('INVOICE_DT').setReadOnly(false);
        f.findField('CKIN_QTY').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        if (f.findField('CKSTATUS').getValue() == '已驗證' && x == "U" || (x == "D"))
            T1Form.down('#submit').setVisible(false);
        else
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
        }/*, {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }*/
        ],
        selModel: {
            checkOnly: false,
            allowDeselect: true,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 110
        }, {
            text: "合約類別",
            dataIndex: 'M_CONTID',
            width: 70
        }, {
            text: "廠商編號",
            dataIndex: 'AGEN_NO',
            width: 180,
            renderer: function (val, meta, record) {
                if (record.data.CKSTATUS == "已驗證") {
                    meta.tdAttr = 'data-qtip=已驗證';
                    return '<font color="blue">' + val + '</font>';
                }
                else
                    return val;
            }
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 90,
            renderer: function (val, meta, record) {
                if (record.data.CHG_CNT > 1) {      // 廠商回覆發票資料>1次
                    meta.tdAttr = 'data-qtip="' + record.data.CHG_LIST + '"';
                    return '<font color="red">' + val + '</font>';
                }
                else
                    return val;
            }
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 70
        }, {
            text: "發票總金額",
            dataIndex: 'INVOICE_TOT',
            width: 90,
            align: 'right'
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 70
        }, {
            text: "中文名稱",
            dataIndex: 'MMNAME_C',
            width: 160
        }, {
            text: "英文名稱",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "合約價",
            dataIndex: 'PO_PRICE',
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "發票驗證數量",
            dataIndex: 'CKIN_QTY',
            style: 'text-align:left',
            width: 90, align: 'right'
        }, {
            text: "訂單數量",
            dataIndex: 'PO_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "單筆金額",
            dataIndex: 'AMOUNT',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "進貨日期",
            dataIndex: 'DELI_DT',
            width: 80
        }, {
            text: "衛署字號",
            dataIndex: 'M_PHCTNCO',
            width: 80
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 180
        }, {
            text: "驗證狀態",
            dataIndex: 'CKSTATUS',
            width: 80,
            renderer: function (val, meta, record) {
                if (record.data.CKSTATUS == "已驗證") {
                    meta.tdAttr = 'data-qtip="' + record.data.CKSTATUS + '"';
                    return '<font color="blue">' + val + '</font>';
                }
                else
                    return val;
            }
        }, {
            xtype: 'hiddenfield',
            dataIndex: 'INVOICE_OLD',
            width: 180
        }, {
            xtype: 'hiddenfield',
            dataIndex: 'TRANSNO_OLD',
            width: 180
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        btnControl('1');
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            T1F1 = f.findField('PO_NO').getValue();
            T1F2 = f.findField('MMCODE').getValue();
            T1F3 = f.findField('TRANSNO').getValue();
            T1F4 = f.findField('CKSTATUS').getValue();
            var u = f.findField('INVOICE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            if (T1F4 == '驗退') {
                Ext.getCmp('btnT1back').setDisabled(true);
            } if (T1F4 == '已驗證') {
                Ext.getCmp('btnT1edit').setDisabled(true);
                Ext.getCmp('btnT1back').setDisabled(true);
                Ext.getCmp('btnT1apply').setDisabled(true);
                Ext.getCmp('btnT1Del').setDisabled(true);
            } else {
                Ext.getCmp('btnT1edit').setDisabled(false);
                Ext.getCmp('btnT1back').setDisabled(false);
                Ext.getCmp('btnT1apply').setDisabled(false);
                Ext.getCmp('btnT1Del').setDisabled(false);
            }
        }
        else {
            T1Form.getForm().reset();
        }
    }
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
        items: [{
            name: 'x',
            xtype: 'hidden'
        }, {
            name: 'TRANSNO',
            xtype: 'hidden'
        }, {
            name: 'CKSTATUS',
            xtype: 'hidden'
        }, {
            xtype: 'hidden',
            name: 'INVOICE_OLD'
        }, {
            xtype: 'hidden',
            name: 'TRANSNO_OLD'
        }, {
            xtype: 'displayfield',
            fieldLabel: '訂單號碼',
            name: 'PO_NO',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商代碼',
            name: 'AGEN_NO'
        }, {
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
            xtype: 'datefield',
            fieldLabel: '發票日期',
            name: 'INVOICE_DT',
            enforceMaxLength: true,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required',
            submitValue: true
        }, {
            fieldLabel: '發票驗證數量',
            name: 'CKIN_QTY',
            allowBlank: false,
            enforceMaxLength: true,
            maxLength: 10,
            maskRe: /[0-9]/,
            fieldCls: 'required',
            readOnly: true,
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '合約價',
            name: 'PO_PRICE',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨數量',
            name: 'DELI_QTY',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '金額',
            name: 'AMOUNT',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '進貨日期',
            name: 'DELI_DT',
            submitValue: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '訂單數量',
            name: 'PO_QTY',
            submitValue: true
        }, {
            xtype: 'textareafield',
            fieldLabel: '備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 300,
            height: 200,
            readOnly: true,
            submitValue: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if ((this.up('form').getForm().findField('INVOICE').getValue() == '' || this.up('form').getForm().findField('INVOICE').getValue() == null) &&
                        (this.up('form').getForm().findField('INVOICE_DT').getValue() == '' || this.up('form').getForm().findField('INVOICE_DT').getValue() == null))
                        Ext.Msg.alert('提醒', '必須輸入發票號碼或發票日期');
                    else {
                        if (this.up('form').getForm().findField('CKIN_QTY').getValue() == '0') {
                            Ext.Msg.alert('提醒', '發票驗證數量不得為0');
                        }
                        else {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            }
                            );
                        }
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                    var data = Ext.decode(action.response.responseText);
                    if (data.success) {
                        if (data.msg.toString() == "") {
                            switch (f2.findField("x").getValue()) {
                                case "I":
                                    // 新增後,將key代入查詢條件,只顯示剛新增的資料
                                    //var v = action.result.etts[0];
                                    //T1Query.getForm().findField('P0').setValue(v.PO_NO);
                                    T1Load();
                                    msglabel('訊息區:資料新增成功');
                                    break;
                                case "U":
                                    var v = action.result.etts[0];
                                    r.set(v);
                                    r.commit();
                                    msglabel('訊息區:資料修改成功');
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
        btnControl('2');

    }

    var T2Form = Ext.widget({
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
                xtype: 'combo',
                fieldLabel: '',
                name: 'T2P0',
                store: st_memo,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                labelAlign: 'right',
                labelWidth: mLabelWidth,
                width: 180,
                listeners: {
                    "select": function (combobox, records, eOpts) {
                        T2Form.getForm().findField('MEMO').setValue(records.data.TEXT);
                    }
                }
            }, {
                xtype: 'textareafield',
                fieldLabel: '請輸入驗退原因',
                name: 'MEMO',
                labelAlign: 'right',
                enforceMaxLength: true,
                //allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                maxLength: 100,
                height: 300
            }
        ],
        buttons: [{
            itemId: 'T2Submit', text: '驗退', handler: function () {
                if (this.up('form').getForm().isValid()) {
                    if (this.up('form').getForm().findField('MEMO').getValue() == '') {
                        Ext.Msg.alert('提醒', '請輸入驗退原因');
                    }
                    else {
                        Ext.Ajax.request({
                            url: '/api/BE0003/UpdateB',
                            method: reqVal_p,
                            params: {
                                PO_NO: T1F1,
                                MMCODE: T1F2,
                                TRANSNO: T1F3,
                                MEMO: T2Form.getForm().findField('MEMO').getValue()
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    hideWin2();
                                    //Ext.MessageBox.alert('訊息', '驗退成功');
                                    msglabel('驗退成功');
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
            }
        },
        {
            itemId: 'cancel', text: '關閉', handler: hideWin2
        }
        ]
    });

    var win2;
    var winActWidth2 = 400;
    var winActHeight2 = 300;
    if (!win2) {
        win2 = Ext.widget('window', {
            title: '驗退',
            closeAction: 'hide',
            width: winActWidth2,
            height: winActHeight2,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T2Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth2 > 0) ? winActWidth2 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight2 > 0) ? winActHeight2 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth2 = width;
                    winActHeight2 = height;
                }
            }
        });
    }

    function showWin2() {
        if (win2.hidden) {
            win2.show();
        }
    }
    function hideWin2() {
        if (!win2.hidden) {
            win2.hide();
        }
    }

    //#region updateWindow
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            'PO_NO',
            'MMCODE',
            'TRANSNO',
            'INVOICE',
            'INVOICE_DT',
            'ITEM_STRING'
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        // pageSize: 20, // 每頁顯示筆數
        //remoteSort: true,
        //sorters: [{ property: 'CREATE_TIME', direction: 'ASC' }], // 預設排序欄位

    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "訊息",
            dataIndex: 'ITEM_STRING',
            width: 180
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "資料流水號",
            dataIndex: 'TRANSNO',
            width: 120
        }, {
            text: "發票號碼",
            dataIndex: 'INVOICE',
            width: 90
        }, {
            text: "發票日期",
            dataIndex: 'INVOICE_DT',
            width: 90
        }]
    });
    var windowHeight = $(window).height();
    var windowWidth = $(window).width();

    function uploadConfirm() {
        var temp_datas = T2Store.getData().getRange();
        var list = [];
        for (var i = 0; i < temp_datas.length; i++) {
            list.push(temp_datas[i].data);
        }
        var myMaskEditWindow = new Ext.LoadMask(Ext.getCmp('uploadWindow'), { msg: '處理中...' });
        myMaskEditWindow.show();
        Ext.Ajax.request({
            url: '/api/BE0003/UploadConfirm',
            method: reqVal_p,
            params: { data: Ext.util.JSON.encode(list) },
            success: function (response) {
                myMaskEditWindow.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('資料上傳成功，請重新查詢');
                    uploadWindow.hide();
                } else {
                    msglabel('資料上傳失敗');
                    Ext.Msg.alert('提醒', data.msg);
                }
                uploadWindow.hide();
            },
            failure: function (response, options) {

            }
        });
    }

    var uploadWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T2Grid],
        width: windowWidth,
        height: windowHeight,
        id: 'uploadWindow',
        xtype: 'form',
        layout: 'form',
        resizable: false,
        draggable: false,
        closable: false,
        layout: {
            type: 'fit',
            padding: 0
        },
        title: "發票資料匯入",
        buttons: [
            {
                text: '確定匯入',
                id: 'uploadConfirm',
                handler: function () {
                    uploadConfirm();
                }
            },
            {
                text: '關閉',
                handler: function () {
                    uploadWindow.hide();
                }
            }
        ]
    });
    uploadWindow.hide();
    //#updateWindow

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
        {
            itemId: 'form',
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
    //T1Load(); // 進入畫面時自動載入一次資料
    if (ADMIN == "Y") { // 針對維護主管
        Ext.getCmp('btnT1Reject').setDisabled(false);
        Ext.getCmp('upload').enable();
        Ext.getCmp('excel').enable();
    }
    else {
        Ext.getCmp('btnT1Reject').setDisabled(true);
        Ext.getCmp('upload').disable();
        Ext.getCmp('excel').disable();
    }

});
