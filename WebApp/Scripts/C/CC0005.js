Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "藥品進貨接收";
    var accLogSet = '/api/CC0005/Create';
    var chkMmcodeGet = '/api/CC0005/ChkMmcode';
    var formDataGet = '/api/CC0005/GetLotNoFormData';
    var isConfirm = false;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var lotnoDataStore = Ext.create('Ext.data.Store', {
        fields: ['PO_NO', 'AGEN_NO', 'AGEN_NAME', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'LOT_NO', { name: 'EXP_DATE', type: 'date' }, 'PO_QTY', 'INQTY', 'BASE_UNIT', 'INVOICE', 'WH_NO', 'PURDATE']
    });

    // 訂貨日期
    var deliDtQueryStore = Ext.create('Ext.data.Store', {
        fields: ['EXTRA1', 'EXTRA2', 'VALUE', 'TEXT', 'COMBITEM']
    });

    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            T1Form.getForm().findField('T1P0').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
    });

    var T1Form = Ext.widget({
        //title: '查詢',
        itemId: 'form2',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        //collapsible: true,
        //hideCollapseTool: true,
        //titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 50%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP0',
                    border: false,
                    layout: {
                        type: 'box',
                        vertical: true,
                        //align: 'right'
                    },
                    items: [
                        {
                            xtype: 'button',
                            text: '藥品揀料核撥',
                            handler: function () {
                                parent.link2('/Form/Index/CD0004', ' 藥品揀料核撥(PDA)', true);
                                //parent.link2('/Form/Index/CE0016','初盤數量輸入作業');
                            },
                            width: '40%',
                        },
                    ]
                },
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    width: '70%',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '條碼編號',
                            name: 'T1P0',
                            id: 'T1P0',
                            fieldCls: 'required',
                            allowBlank: false,
                            enforceMaxLength: true,
                            maxLength: 20,
                            width: '90%',
                            margin: '2 4 0 4',
                            plugins: 'responsive',
                            labelStyle: 'width: 43%',
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length >= 8) {
                                        chkMmcode(nValue);
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            iconCls: 'TRACamera',
                            width: '9%',
                            handler: function () {
                                showScanWin(viewport);
                            }
                        }
                    ]
                }, {
                    xtype: 'combo',
                    store: deliDtQueryStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    id: 'T1P1',
                    name: 'T1P1',
                    margin: '2 4 0 4',
                    width: '90%',
                    labelStyle: 'width: 39%',
                    fieldLabel: '訂貨日期',
                    fieldCls: 'required',
                    allowBlank: false,
                    disabled: true,
                    queryMode: 'local',
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 300);
                            }
                        },
                        select: function (field, record, eOpts) {
                            setT1FormData(record.get('EXTRA1'), record.get('EXTRA2'), record.get('VALUE'));
                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    padding: '1vh 0 0 0',
                    layout: 'hbox',
                    width: '25%',
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        width: '50%',
                        margin: '1',
                        handler: function () {
                            msglabel('');
                            chkMmcode(T1Form.getForm().findField('T1P0').getValue());
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        width: '50%',
                        margin: '1',
                        handler: function () {
                            msglabel('');
                            T1Form.down('#DETAIL_FORM').setVisible(false);
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('T1P0').focus();
                        }
                    }]
                }
            ]
        }, {
            xtype: 'panel',
            itemId: 'DETAIL_FORM',
            border: true,
            hidden: true,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    labelStyle: 'width: 25%',
                    width: '100%',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '品名',
                            name: 'MMNAME_E',
                            margin: '1 0 4 0',
                            border: 1,
                            style: {
                                borderColor: 'gray',
                                borderStyle: 'solid'
                            },
                            labelStyle: 'width: 25%',
                            width: '92%'
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            iconCls: 'TRASearch',
                            width: '7%',
                            handler: function () {
                                popItemDetail();
                            }
                        }
                    ]
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '訂單編號',
                    name: 'PO_NO',
                    maxLength: 20,
                    margin: '2 4 0 4',
                    labelStyle: 'width: 34%',
                    width: '85%',
                }, {
                    xtype: 'textfield',
                    fieldLabel: '批號',
                    name: 'LOT_NO',
                    id: 'LOT_NO',
                    enforceMaxLength: true,
                    maxLength: 20,
                    fieldCls: 'required',
                    allowBlank: false,
                    margin: '2 4 0 4',
                    enableKeyEvents: true,
                    labelStyle: 'width: 34%',
                    width: '85%',
                    listeners: {
                        keydown: function (field, e, eOpts) {
                            if (e.keyCode == '13' && isConfirm == false)
                                accConfirmChk();
                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    labelStyle: 'width: 25%',
                    width: '100%',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '效期',
                            name: 'EXP_DATE',
                            id: 'EXP_DATE',
                            fieldCls: 'required',
                            allowBlank: false,
                            margin: '0 4 0 4',
                            enableKeyEvents: true,
                            labelStyle: 'width: 40%',
                            width: '85%',
                            listeners: {
                                focus: function (field, event, eOpts) {
                                    if (!field.isExpanded) {
                                        setTimeout(function () {
                                            field.expand();
                                        }, 300);
                                    }
                                },
                                change: function (field, nDate, oDate, eOpts) {
                                    var f = T1Form.getForm();
                                    if (f.findField('EXP_DATE').rawValue <= getShiftDaysChtDate(365) && f.findField('EXP_DATE').rawValue != '')
                                        f.findField('EXP_MARK').setHtml('<b><font color="red" size="5">＊</font></b>');
                                    else
                                        f.findField('EXP_MARK').setHtml('');
                                },
                                keydown: function (field, e, eOpts) {
                                    if (e.keyCode == '13' && isConfirm == false)
                                        accConfirmChk();
                                }
                            }
                        }, {
                            xtype: 'displayfield',
                            name: 'EXP_MARK',
                            id: 'EXP_MARK',
                            width: '5%'
                        }
                    ]
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    labelStyle: 'width: 25%',
                    width: '100%',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '進貨數量',
                            name: 'INQTY',
                            id: 'INQTY',
                            //enforceMaxLength: true,
                            //maxLength: 8,
                            maskRe: /[0-9]/,
                            regex: /^([1-9][0-9]*|0)$/,
                            regexText: '需為自然數',
                            fieldCls: 'required',
                            allowBlank: false,
                            margin: '2 4 0 4',
                            enableKeyEvents: true,
                            labelStyle: 'width: 40%',
                            width: '85%',
                            listeners: {
                                keydown: function (field, e, eOpts) {
                                    if (e.keyCode == '13' && isConfirm == false)
                                        accConfirmChk();
                                }
                            }
                        }
                        // 依藥庫要求,下月入帳不採用勾選方式,而是進貨數量輸入0即用預設值做下月入帳
                        //, {
                        //    xtype: 'checkbox',
                        //    boxLabel: '<font size="3vmin">下個月入帳</font>',
                        //    padding: '0 0 0 2',
                        //    labelAlign: 'right',
                        //    labelSeparator: '',
                        //    name: 'NEXTMON',
                        //    id: 'NEXTMON',
                        //    plugins: 'responsive',
                        //    responsiveConfig: {
                        //        'width < 600': {
                        //            labelStyle: 'width: 45%',
                        //            width: '50%'
                        //        },
                        //        'width >= 600': {
                        //            labelStyle: 'width: 75%',
                        //            width: '50%'
                        //        }
                        //    }
                        //}
                    ]
                }, {
                    xtype: 'panel',
                    border: false,
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    labelStyle: 'width: 25%',
                    width: '100%',
                    margin: '-8 0 1 0',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '訂單數量',
                            name: 'PO_QTY',
                            margin: '2 4 0 4',
                            editable: false,
                            laberWidth: false,
                            labelStyle: 'width: 40%',
                            width: '85%',
                            enableKeyEvents: true,
                            listeners: {
                                keydown: function (field, e, eOpts) {
                                    if (e.keyCode == '13' && isConfirm == false)
                                        T1Form.getForm().findField('INQTY').focus();
                                }
                            }
                        }
                    ]
                }, {
                    xtype: 'hidden',
                    name: 'MMCODE',
                    id: 'MMCODE'
                }, {
                    xtype: 'hidden',
                    name: 'PO_NO',
                    id: 'PO_NO'
                }, {
                    xtype: 'hidden',
                    name: 'AGEN_NO',
                    id: 'AGEN_NO'
                }, {
                    xtype: 'hidden',
                    name: 'WH_NO',
                    id: 'WH_NO'
                }, {
                    xtype: 'hidden',
                    name: 'PURDATE',
                    id: 'PURDATE'
                }, {
                    xtype: 'hidden',
                    name: 'PH_REPLY_SEQ',
                    id: 'PH_REPLY_SEQ'
                }, {
                    xtype: 'hidden',
                    name: 'INVOICE',
                    id: 'INVOICE'
                }, {
                    xtype: 'hidden',
                    name: 'INQTY_O', // 接收數量(原值)
                    id: 'INQTY_O'
                }, {
                    xtype: 'button',
                    itemId: 'accConfirmBtn',
                    text: '進貨接收確認',
                    width: '100%',
                    margin: '4 4 0 2',
                    handler: function () {
                        accConfirmChk();
                    }
                }
            ]
        }]
    });

    var accConfirmChk = function () {
        var f = T1Form.getForm();
        var inqty = parseInt(f.findField('INQTY').getValue());
        var poqty = parseInt(f.findField('PO_QTY').getValue());
        if (inqty > poqty)
            Ext.Msg.alert('訊息', '進貨數量 > 訂購數量，不可以接收。');
        else if (inqty < poqty) {
            var temp = '進貨數量 < 訂購數量，確認接收?';
            if (inqty == 0) {
                temp = '進貨數量 = 0，確認下個月入帳?';
            }
            Ext.MessageBox.confirm('確認', temp, function (btn, text) {
                if (btn === 'yes') {
                    T1Submit();
                } else {
                    T1Form.getForm().findField('INQTY').focus();
                }
            });
        }
        else if (f.findField('LOT_NO').getValue() == '' || f.findField('EXP_DATE').getValue() == null)
            Ext.Msg.alert('訊息', '批號、效期，不可空白。');
        else if (!f.findField('EXP_DATE').isValid())
            Ext.Msg.alert('訊息', '效期格式不正確，應為yyymmdd格式，</br>例如1080101。');
        else if (!f.findField('INQTY').isValid())
            Ext.Msg.alert('訊息', '進貨數量格式不正確。');
        else
            T1Submit();
    }

    //var M_CONTIDtoMsg = function (m_contid) {
    //    if (m_contid == '0')
    //        return '合約';
    //    else if (m_contid == '2')
    //        return '非合約';
    //    else
    //        return m_contid;
    //}

    // 檢查院內碼
    function chkMmcode(mmcode) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        var myMask_hide = new Ext.util.DelayedTask(function () {
            myMask.hide();
        });
        myMask.show();
        Ext.Ajax.request({
            url: chkMmcodeGet,
            params: {
                p0: T1Form.getForm().findField('T1P0').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    var tb_data = data.etts;
                    deliDtQueryStore.removeAll();
                    T1Form.getForm().findField('T1P1').setValue('');
                    T1Form.getForm().findField('T1P1').setDisabled(true);
                    T1Form.down('#DETAIL_FORM').setVisible(false);
                    if (rtnStr != '' && rtnStr != null) {
                        Ext.Msg.alert('訊息', rtnStr);
                    }
                    else if (tb_data.length == 0) {
                        Ext.Msg.alert('訊息', '目前查無廠商回覆檔對應資料');
                    }
                    else if (tb_data.length > 0) {
                        if (tb_data.length > 0) {
                            for (var i = 0; i < tb_data.length; i++) {
                                deliDtQueryStore.add({ VALUE: tb_data[i].VALUE, TEXT: tb_data[i].TEXT, EXTRA1: tb_data[i].EXTRA1, EXTRA2: tb_data[i].EXTRA2, COMBITEM: tb_data[i].COMBITEM });
                            }
                            // 訂貨日期enable,預設為第一筆
                            T1Form.getForm().findField('T1P1').setDisabled(false);
                            T1Form.getForm().findField('T1P1').setValue(tb_data[0].VALUE);
                            T1Form.down('#DETAIL_FORM').setVisible(true);
                            setT1FormData(tb_data[0].EXTRA1, tb_data[0].EXTRA2, tb_data[0].VALUE);
                            T1Form.getForm().findField('PO_QTY').focus();
                            isConfirm = false;
                        }
                    }
                }
                myMask_hide.delay(500);  // 0.5秒
            },
            failure: function (response, options) {
                myMask.hide();
                Ext.Msg.alert('訊息', '網路問題，請再試一次');
                T1Form.getForm().findField('T1P0').focus();
            }
        });
    }

    function T1Submit() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        var myMask_hide = new Ext.util.DelayedTask(function () {
            myMask.hide();
        });
        myMask.show();
        var f = T1Form.getForm();
        var isNextmon = false;
        if (f.findField('INQTY').getValue() == '0')
            isNextmon = true; // 進貨數量填0,則下月入帳(藥庫要求)
        Ext.Ajax.request({
            url: accLogSet,
            params: {
                po_no: f.findField('PO_NO').getValue(),
                agen_no: f.findField('AGEN_NO').getValue(),
                mmcode: f.findField('MMCODE').getValue(),
                lot_no: f.findField('LOT_NO').getValue(),
                exp_date: f.findField('EXP_DATE').getValue(),
                inqty: f.findField('INQTY').getValue(),
                inqty_o: f.findField('INQTY_O').getValue(),
                wh_no: f.findField('WH_NO').getValue(),
                purdate: f.findField('PURDATE').getValue(),
                nextmon: isNextmon,
                ph_reply_seq: f.findField('PH_REPLY_SEQ').getValue(),
                po_qty: f.findField('PO_QTY').getValue(),
                acc_qty: f.findField('INQTY').getValue(),
                invoice: f.findField('INVOICE').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == '接收數量存檔..成功') {
                        // Ext.Msg.alert('訊息', '接收數量存檔成功');
                        msglabel(f.findField('MMCODE').getValue() + '接收數量存檔成功');
                        isConfirm = true;
                        T1Form.down('#accConfirmBtn').setDisabled(true);
                        // 接收後,清除查詢資料,focus到查詢條件的院內碼 
                        T1Form.down('#DETAIL_FORM').setVisible(false);
                        deliDtQueryStore.removeAll();
                        T1Form.getForm().findField('T1P1').setValue('');
                        T1Form.getForm().findField('T1P1').setDisabled(true);
                        T1Form.getForm().findField('T1P0').setValue('');
                        T1Form.getForm().findField('T1P0').focus();
                    }
                    else {
                        msglabel('失敗-' + data.msg);
                        Ext.Msg.alert('訊息', '失敗-' + data.msg);
                    }
                }
                myMask_hide.delay(500);  // 0.5秒
            },
            failure: function (response, options) {
                myMask.hide();
                Ext.Msg.alert('訊息', '失敗-' + data.msg);
            }
        });
    }

    function setT1FormData(po_no, mmcode, seq) {
        Ext.Ajax.request({
            url: formDataGet,
            params: {
                po_no: po_no,
                mmcode: mmcode,
                seq: seq
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    lotnoDataStore.removeAll();
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            var exp_date = tb_data[i].EXP_DATE.toString();
                            if (exp_date.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || exp_date.substring(0, 24) == 'Mon Jan 01 0001 00:00:00'
                                || exp_date.substring(0, 19) == '0001-01-01T00:00:00') {
                                tb_data[i].EXP_DATE = '';  // 若資料庫值為null,在這邊另外做清除
                            }
                        }

                        for (var i = 0; i < tb_data.length; i++) {
                            lotnoDataStore.add({
                                PO_NO: tb_data[i].PO_NO, MMCODE: tb_data[i].MMCODE, MMNAME_C: tb_data[i].MMNAME_C, MMNAME_E: tb_data[i].MMNAME_E, AGEN_NO: tb_data[i].AGEN_NO, AGEN_NAME: tb_data[i].AGEN_NAME,
                                LOT_NO: tb_data[i].LOT_NO, EXP_DATE: tb_data[i].EXP_DATE, PO_QTY: tb_data[i].PO_QTY, BASE_UNIT: tb_data[i].BASE_UNIT, INQTY: tb_data[i].INQTY, INVOICE: tb_data[i].INVOICE,
                                WH_NO: tb_data[i].WH_NO, PURDATE: tb_data[i].PURDATE
                            });
                        }

                        // 將取得資料代入Form
                        var f = T1Form.getForm();
                        f.loadRecord(lotnoDataStore.getAt(0));
                        f.findField('INQTY_O').setValue(f.findField('INQTY').getValue());
                        T1Form.getForm().findField('PH_REPLY_SEQ').setValue(seq);
                        T1Form.down('#accConfirmBtn').setDisabled(false);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    var callableWin = null;
    popItemDetail = function () {
        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                //id: 'itemDetailCard',
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'itemdetailform',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 45%',
                        width: '95%'
                    },
                    items: [
                        {
                            xtype: 'container',
                            layout: 'vbox',
                            padding: '2vmin',
                            scrollable: true,
                            items: [
                                {
                                    xtype: 'displayfield',
                                    fieldLabel: '廠商編號',
                                    name: 'AGEN_NO',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '廠商名稱',
                                    name: 'AGEN_NAME',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '訂單編號',
                                    name: 'PO_NO',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '中文品名',
                                    name: 'MMNAME_C',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '英文品名',
                                    name: 'MMNAME_E',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '計量單位',
                                    name: 'BASE_UNIT',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '發票號碼',
                                    name: 'INVOICE',
                                    readOnly: true
                                }, {
                                    xtype: 'textfield',
                                    name: 'COMMAND',
                                    readOnly: false,
                                    enableKeyEvents: true,
                                    width: '10%',
                                    listeners: {
                                        keydown: function (field, e, eOpts) {
                                            if (e.keyCode == '13')
                                                popMainform.down('#winclosed').click();
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                }],
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">關閉</font>',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popMainform, '院內碼明細', viewport.width * 0.9, viewport.height * 0.9);

            popMainform.down('#itemdetailform').loadRecord(lotnoDataStore.getAt(0));

            setTimeout(function () {
                popMainform.down('#itemdetailform').getForm().findField('COMMAND').focus();
            }, 300);
        }
        callableWin.show();
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
            //id: 'itemDetailCard',
            //itemId: 't1Grid',
            id: 't1Grid',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Form]
        }]
    });

    T1Form.getForm().findField('T1P0').focus();
    Ext.getDoc().dom.title = T1Name;
});
