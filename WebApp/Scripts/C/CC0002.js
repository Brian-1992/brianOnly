Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    //var T1Name = "進貨驗收作業";
    //var T1Name = "中央庫進貨接收";
    var T1Name = "衛材進貨接收";
    var accLogSet = '/api/CC0002/Create';  //輸入[廠商代碼],有批號效期
    var accAllSet = '/api/CC0002/AccAllCreate';
    var scanDistLogSet = '/api/CC0002/UpdateScanDist';
    var distLogSet = '/api/CC0002/UpdateDist';//有批號效期
    var AgennoComboGet = '/api/BC0002/GetAgennoCombo';
    var chkAgennoGet = '/api/CC0002/ChkAgenno';
    var chkBarcodeGet = '/api/CC0002/ChkBarcode';
    var chkMmcodeGet = '/api/CC0002/ChkMmcode';
    var hadDistQtyGet = '/api/CC0002/GetHadDistQty';
    // var lotNoComboGet = '/api/CC0002/GetLotNoCombo';
    var lotNoDataGet = '/api/CC0002/GetLotNoData';
    var udiDataGet = '/api/CC0002/GetUdiData';

    var T1Rec = 0;
    var T1LastRec = null;
    var T2Rec = 0;
    var T2LastRec = null;
    var T21Rec = 0;
    var T21LastRec = null;
    // var LastSeq = null;
    var T3Rec = 0;
    var T3LastRec = null;
    var UdiRec = null;

    var justShow = false;
    var isACC = false; // 此訂單已按[一鍵接收]

    var T3Store_url = '/api/CC0002/DistAll';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var lotnoDataStore = Ext.create('Ext.data.Store', {
        fields: ['PO_NO', 'MMCODE', 'LOT_NO', { name: 'EXP_DATE', type: 'date' }, 'BW_SQTY', 'INQTY']
    });

    var scanView = 0; // 掃描後值要代入哪個畫面: 0:主畫面的條碼; 1:第二層畫面的條碼
    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            if (scanView == 0)
                T1Query.getForm().findField('P2').setValue(CameraSharedData.selItem);
            else if (scanView == 1)
                T2Query.getForm().findField('T2P0').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
    });

    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'P0',
        fieldLabel: '廠商編號',
        fieldCls: 'required',
        allowBlank: false,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        width: '20%',
        margin: '1 0 1 0',
        padding: '0 4 0 4',
        selectOnFocus: true,
        plugins: 'responsive',
        responsiveConfig: {
            'width < 600': {
                labelStyle: 'width: 45%',
            },
            'width >= 600': {
                labelStyle: 'width: 40%',
            }
        },
        listeners: {
            focus: function (field, event, eOpts) {
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

    var T1Query = Ext.widget({
        title: '查詢',
        itemId: 'form',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 40%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: {
                type: 'box',
                vertical: true,
                align: 'stretch'
            },
            items: [
                T1FormAgenno, {
                    xtype: 'datefield',
                    id: 'P11',
                    name: 'P11',
                    fieldLabel: '',
                    margin: '1 0 1 0',
                    selectOnFocus: true,
                    value: new Date(),
                    hidden: true
                }, {
                    xtype: 'datefield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '訂單日期(起)',
                    margin: '1 0 1 0',
                    selectOnFocus: true,
                    plugins: 'responsive',
                    fieldCls: 'required',
                    allowBlank: false,
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 45%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                        }
                    },
                    listeners: {
                        change: function (fiels, nValue, oValue, eOpts) {

                        },
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 300);
                            }
                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 35%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                        }
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            id: 'P2',
                            name: 'P2',
                            fieldLabel: '條碼',
                            margin: '1 0 1 0',
                            selectOnFocus: true,
                            width: '90%',
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    labelStyle: 'width: 35%',
                                },
                                'width >= 600': {
                                    labelStyle: 'width: 40%',
                                }
                            },
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    T1Store.removeAll();
                                    T1Query.getForm().findField('P0').setValue('');
                                    viewport.down('#agenform').setTitle('');
                                    if (nValue.length >= 6) {
                                        T1getUdiData(nValue); // 先檢查是否為UDI條碼
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            iconCls: 'TRACamera',
                            width: '9%',
                            handler: function () {
                                scanView = 0;
                                showScanWin(viewport);
                            }
                        }
                    ]
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'hbox',
                            }
                        },
                        'width >= 600': {
                            layout: {
                                type: 'hbox',
                                vertical: false
                            }
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            },
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            if (f.findField('P0').getValue() == '')
                                Ext.Msg.alert('提醒', '查詢條件的廠商編號為必填');
                            else {
                                chkAgenno();
                            }
                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            },
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            // f.findField('P0').focus();
                            msglabel('');
                        }
                    }]
                }
            ]
        }]
    });

    var T1AGEN = Ext.widget({
        title: '　',
        itemId: 'agenform',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: false,
        hideCollapseTool: true,
        titleCollapse: false
    });

    var T2Query = Ext.widget({
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
            labelStyle: 'width: 30%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    layout: {
                        type: 'hbox',
                        vertical: false
                    },
                    responsiveConfig: {
                        'width < 600': {
                            width: '70%'
                        },
                        'width >= 600': {
                            width: '30%'
                        }
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '條碼',
                            name: 'T2P0',
                            id: 'T2P0',
                            enforceMaxLength: true,
                            // maxLength: 13,
                            width: '90%',
                            margin: '2 4 0 4',
                            selectOnFocus: true,
                            listeners: {
                                change: function (field, nValue, oValue, eOpts) {
                                    if (nValue.length >= 6) {
                                        getUdiData();
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            itemId: 'scanBtn',
                            iconCls: 'TRACamera',
                            width: '9%',
                            handler: function () {
                                scanView = 1;
                                showScanWin(viewport);
                            }
                        }
                    ]
                }, {
                    xtype: 'panel',
                    border: false,
                    padding: '1vh 0 0 0',
                    layout: 'hbox',
                    width: '30%',
                    items: [{
                        xtype: 'button',
                        text: '查詢',
                        width: '50%',
                        margin: '1',
                        handler: function () {
                            T2Rec = 0;
                            T21Rec = 0;
                            T2LastRec = null;
                            T21LastRec = null;
                            T2Store.load();
                            T21Store.load();
                            T2Tool.down('#distConfirmBtn').setDisabled(true);
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        width: '50%',
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('T2P0').focus();
                        }
                    }]
                }
            ]
        }]
    });

    var T3Form = Ext.widget({
        itemId: 'form3',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: true,
        hideCollapseTool: true,
        titleCollapse: true,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 40%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'box',
                        vertical: true,
                        align: 'stretch'
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'box',
                        vertical: false
                    }
                }
            },
            items: [
                {
                    xtype: 'label',
                    html: '<b><font color="red" size="3vmin">非庫備申請明細</font></b>',
                    style: 'display:inline-block;text-align:center',
                    margin: '4 0 4 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 25%',
                            width: '70%'
                        },
                        'width >= 600': {
                            labelStyle: 'width: 45%',
                            width: '30%'
                        }
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '訂單號碼',
                    name: 'PO_NO',
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                            width: '70%'
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                            width: '30%'
                        }
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '院內碼',
                    name: 'MMCODE',
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                            width: '70%'
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                            width: '30%'
                        }
                    }
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'box',
                        vertical: true,
                        align: 'stretch'
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'box',
                        vertical: false
                    }
                }
            },
            items: [
                {
                    xtype: 'displayfield',
                    fieldLabel: '中文品名',
                    name: 'MMNAME_C',
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                            width: '70%'
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                            width: '30%'
                        }
                    }
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '進貨接收',
                    name: 'ACC_QTY',
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 30%',
                            width: '70%'
                        },
                        'width >= 600': {
                            labelStyle: 'width: 40%',
                            width: '30%'
                        }
                    }
                }
                //, {
                //    xtype: 'displayfield',
                //    fieldLabel: '已分配量',
                //    name: 'HAD_DIST_QTY',
                //    margin: '1 0 1 0',
                //    plugins: 'responsive',
                //    responsiveConfig: {
                //        'width < 600': {
                //            labelStyle: 'width: 25%',
                //            width: '70%'
                //        },
                //        'width >= 600': {
                //            labelStyle: 'width: 40%',
                //            width: '30%'
                //        }
                //    }
                //}
            ]
        }]
    });

    // 2019/07/01指示,改回點選一筆資料即進入下一頁
    //var T1Footer = Ext.widget({
    //    itemId: 'footerForm1',
    //    xtype: 'form',
    //    layout: 'form',
    //    border: false,
    //    autoScroll: true,
    //    width: '100%',
    //    fieldDefaults: {
    //        xtype: 'textfield',
    //        labelAlign: 'right',
    //        labelWidth: false,
    //        labelStyle: 'width: 40%',
    //        width: '30%'
    //    },
    //    items: [{
    //        xtype: 'panel',
    //        border: false,
    //        margin: '1 0 1 0',
    //        plugins: 'responsive',
    //        responsiveConfig: {
    //            'width < 600': {
    //                layout: {
    //                    type: 'hbox',
    //                }
    //            },
    //            'width >= 600': {
    //                layout: {
    //                    type: 'hbox',
    //                    vertical: false
    //                }
    //            }
    //        },
    //        items: [{
    //            xtype: 'button',
    //            itemId: 'rtnQueryBtn',
    //            text: '返回查詢',
    //            disabled: true,
    //            plugins: 'responsive',
    //            responsiveConfig: {
    //                'width < 600': {
    //                    width: '50%'
    //                },
    //                'width >= 600': {
    //                    width: 250
    //                }
    //            },
    //            margin: '1',
    //            handler: function () {
    //                T1Query.expand();
    //                T1Grid.down('#rtnQueryBtn').setDisabled(true);
    //            }
    //        }, {
    //            xtype: 'button',
    //            itemId: 'poConfirmBtn',
    //            text: '訂單號碼確認',
    //            disabled: true,
    //            plugins: 'responsive',
    //            responsiveConfig: {
    //                'width < 600': {
    //                    width: '50%'
    //                },
    //                'width >= 600': {
    //                    width: 250
    //                }
    //            },
    //            margin: '1',
    //            handler: function () {
    //                showItemList();
    //            }
    //        }]
    //    }]
    //});

    var T2Tool = Ext.widget({
        itemId: 'formTool2',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 40%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            //margin: '1 0 1 0',
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'hbox',
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'hbox',
                        vertical: false
                    }
                }
            },
            items: [{
                xtype: 'button',
                itemId: 'distConfirmBtn',
                id: 'distConfirmBtn',
                text: '一鍵接收',
                //disabled: true,
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        width: '25%'
                    },
                    'width >= 600': {
                        width: 180
                    }
                },
                margin: '1',
                handler: function () {
                    popAccAllConfirm();
                    msglabel('');
                }
            }, {
                xtype: 'button',
                text: '返回',
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        width: '20%'
                    },
                    'width >= 600': {
                        width: 150
                    }
                },
                margin: '1',
                handler: function () {
                    showPoList();
                    msglabel('');
                }
            }]
        }]
    });

    var T21Tool = Ext.widget({
        itemId: 'formTool21',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 40%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            //margin: '1 0 1 0',
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'hbox',
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'hbox',
                        vertical: false
                    }
                }
            },
            items: [{
                xtype: 'label',
                padding: '4 4 0 2',
                html: '<b><font color="red">批號效期管理品項</font></b>'
            }, {
                xtype: 'button',
                text: '返回',
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        width: '20%'
                    },
                    'width >= 600': {
                        width: 150
                    }
                },
                margin: '1',
                handler: function () {
                    showPoList();
                    msglabel('');
                }
            }]
        }]
    });

    var T3Tool = Ext.widget({
        itemId: 'formTool3',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 40%',
            width: '30%'
        },
        items: [{
            xtype: 'panel',
            border: false,
            //margin: '1 0 1 0',
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'hbox',
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'hbox',
                        vertical: false
                    }
                }
            },
            items: [{
                xtype: 'button',
                itemId: 'mmcodeDistBtn',
                text: '分配',
                disabled: true,
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        width: '20%'
                    },
                    'width >= 600': {
                        width: 150
                    }
                },
                margin: '1',
                handler: function () {
                    // popDistForm('單位非庫備分配');
                    msglabel('');

                    setTimeout(function () {
                        // 計算分配量總和
                        var distQtySum = 0;
                        var chkMSG = "";
                        var AGEN_ACC_QTY = 0;  //輸入[廠商代碼],有批號效期 記錄[接收量]比對[分配量]用
                        for (var i = 0; i < T3Store.data.items.length; i++) {
                            var itemDistQty = 0;
                            if (T3Store.data.items[i].data.DIST_QTY) {
                                itemDistQty = T3Store.data.items[i].data.DIST_QTY;
                                if ((parseInt(T3Store.data.items[i].data.PR_QTY) - parseInt(T3Store.data.items[i].data.DSUM_QTY)) < parseInt(T3Store.data.items[i].data.DIST_QTY))
                                    chkMSG += T3Store.data.items[i].data.INID_NAME + "->[分配量]超過[申請量]-[已分配量];</br>";
                            }
                            distQtySum += parseInt(itemDistQty);
                        }
                        if (T3Store.data.items.length != 0)
                            AGEN_ACC_QTY = parseInt(T3Form.getForm().findField('ACC_QTY').getValue());
                        if (chkMSG != "")
                            Ext.Msg.alert('訊息', chkMSG);
                        //else if (T3Store.data.items.length != 0 && distQtySum == 0)
                        //    Ext.Msg.alert('訊息', '[分配量總和]不可等於 0');
                        //else if (distQtySum != AGEN_ACC_QTY)
                        //    Ext.Msg.alert('訊息', '[分配量總和]=' + distQtySum + ' 不等於[接收量]=' + AGEN_ACC_QTY);
                        else {
                            Ext.MessageBox.confirm('訊息', '是否確認分配?', function (btn, text) {
                                if (btn === 'yes') {
                                    var conDistQty = '';
                                    var conSeq = '';
                                    var conInid = '';
                                    for (var i = 0; i < T3Store.data.items.length; i++) {
                                        if (i == T3Store.data.items.length - 1) {
                                            // 最後一筆資料不加分割符號
                                            conDistQty = conDistQty + T3Store.data.items[i].data.DIST_QTY;
                                            conSeq = conSeq + T3Store.data.items[i].data.SEQ;
                                            conInid = conInid + T3Store.data.items[i].data.INID;
                                        }
                                        else {
                                            // 若尚有資料則後面加上^分割符號
                                            conDistQty = conDistQty + T3Store.data.items[i].data.DIST_QTY + '^';
                                            conSeq = conSeq + T3Store.data.items[i].data.SEQ + '^';
                                            conInid = conInid + T3Store.data.items[i].data.INID + '^';
                                        }
                                    }
                                    var f = T3Form.getForm();
                                    Ext.Ajax.request({
                                        url: distLogSet,
                                        params: {
                                            dist_qty: conDistQty,
                                            po_no: f.findField('PO_NO').getValue(),
                                            mmcode: f.findField('MMCODE').getValue(),
                                            seq: conSeq,
                                            inid: conInid
                                        },
                                        method: reqVal_p,
                                        success: function (response) {
                                            //T3Store.load();
                                            T3Tool.down('#mmcodeDistBtn').setDisabled(true);
                                            T3Tool.down('#t3Back_id').click();
                                            msglabel('非庫備分配紀錄新增成功');
                                        },
                                        failure: function (response, options) {
                                        }
                                    });
                                }
                            });

                        }
                    }, 300); // 給予一點延遲,以在編輯完一筆分配量,尚未完成edit就直接按分配,也能取得編輯後的值
                }
            }, {
                xtype: 'button',
                itemId: 't3Back_id',
                text: '返回',
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        width: '20%'
                    },
                    'width >= 600': {
                        width: 150
                    }
                },
                margin: '1',
                handler: function () {
                    T3Tool.down('#mmcodeDistBtn').setDisabled(true);
                    showItemList();
                    msglabel('');
                }
            }]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.CC0002', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, operation, eOpts) {
                if (records.length == 0) {
                    Ext.Msg.alert('訊息', '查無對應的訂單資料。');
                }
                T1Query.getForm().findField('P2').setValue('');
            }
        }
    });

    var T2Store = Ext.create('WEBAPP.store.CC0002D', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['PO_NO'],
                    p1: T2Query.getForm().findField('T2P0').getValue(),
                    p2: T1LastRec.data['AGEN_NO']
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (store.data.items.length > 0 && isACC == false)
                    T2Tool.down('#distConfirmBtn').setDisabled(false);
                else
                    T2Tool.down('#distConfirmBtn').setDisabled(true);

                for (var i = 0; i < store.data.items.length; i++) {
                    // 將store內每一筆資料,預設為REPLY_QTY或PO_QTY - PO_ACC_QTY
                    if (parseInt(store.data.items[i].data['REPLY_QTY']) > 0) {
                        store.data.items[i].data['ACC_QTY'] = store.data.items[i].data['REPLY_QTY'];
                    }
                    else
                        store.data.items[i].data['ACC_QTY'] = parseInt(store.data.items[i].data['PO_QTY']) - parseInt(store.data.items[i].data['PO_ACC_QTY']);
                }

                T2Grid.setStore(store);
            },
            sort: function (store, eOpts) {
                T2Rec = 0;
                T2LastRec = null;
                T21LastRec = null;
            }

        }
    });

    var T21Store = Ext.create('WEBAPP.store.CC0002DE', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['PO_NO'],
                    p1: T2Query.getForm().findField('T2P0').getValue(),
                    p2: T1LastRec.data['AGEN_NO']
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                for (var i = 0; i < store.data.items.length; i++) {
                    // 將store內每一筆資料,預設為REPLY_QTY或PO_QTY - PO_ACC_QTY
                    if (parseInt(store.data.items[i].data['REPLY_QTY']) > 0) {
                        store.data.items[i].data['ACC_QTY'] = store.data.items[i].data['REPLY_QTY'];
                    }
                    else
                        store.data.items[i].data['ACC_QTY'] = parseInt(store.data.items[i].data['PO_QTY']) - parseInt(store.data.items[i].data['PO_ACC_QTY']);
                }

                T21Grid.setStore(store);

                if (store.data.items.length > 0)
                    T21Grid.setVisible(true);
                else
                    T21Grid.setVisible(false);
            },
            sort: function (store, eOpts) {
                T21Rec = 0;
                T21LastRec = null;
            }

        }
    });

    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['PO_NO', 'MMCODE', 'SEQ', 'PR_DEPT', 'INID', 'INID_NAME', 'PR_QTY', 'LOT_NO', 'EXP_DATE', 'MMNAME_C', 'DSUM_QTY']
    });
    var T3Store = Ext.create('Ext.data.Store', {
        model: T3Model,
        pageSize: 1000,
        remoteSort: true,
        sorters: [{ property: 'INID', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T3Store_url,
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var parPoNo = '';
                var parMmcode = '';
                if (T21LastRec) {
                    // 批號效期管理品項
                    parPoNo = T21LastRec.data['PO_NO'];
                    parMmcode = T21LastRec.data['MMCODE'];
                }
                else {
                    // 直接掃描條碼(於.load時代參數)
                }
                var np = {
                    p0: parPoNo,
                    p1: parMmcode,
                    p2: ''
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (store.data.items.length > 0)
                    T3Tool.down('#mmcodeDistBtn').setDisabled(false);
                else
                    T3Tool.down('#mmcodeDistBtn').setDisabled(true);
            },
            sort: function (store, eOpts) {
                T3Rec = 0;
                T3LastRec = null;
            }

        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        plugins: 'bufferedrenderer',
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1AGEN]
        }
            //, {
            //dock: 'bottom',
            //xtype: 'toolbar',
            //items: [T1Footer]
            //}
        ],
        columns: [{
            text: "類別",
            dataIndex: 'M_CONTID',
            width: '25%',
            renderer: function (val, meta, record) {
                return M_CONTIDtoMsg(val);
            }
        }, {
            text: "訂單號碼",
            dataIndex: 'PO_NO',
            width: '65%'
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                //T1Grid.down('#poConfirmBtn').setDisabled(T1Rec === 0);
            },
            //click: {
            //    element: 'el',
            //    fn: function () {
            //        if (T1LastRec != null) {
            //            showItemList();
            //        }
            //    }
            //},
            itemclick: function (view, record, item, index, e, eOpts) {
                if (T1LastRec != null) {
                    showItemList();
                }
            }
        }
    });

    var T21Grid = Ext.create('Ext.grid.Panel', {
        // 院內碼清單(下半部-批號效期管理品項)
        height: 200,
        layout: 'fit',
        closable: false,
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        sortableColumns: false,
        plugins: 'bufferedrenderer',
        cls: 'T1',
        selModel: "cellmodel",
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T21Tool]
        }],
        columns: [
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '29%'
            },
            //{
            //    text: "已收/訂單",
            //    dataIndex: 'M_STOREID',
            //    width: '36%',
            //    renderer: function (val, meta, record) {
            //        return record.data['PO_ACC_QTY'] + '/' + record.data['PO_QTY'] + ' ' + record.data['M_PURUN']
            //    }
            //},
            {
                text: "訂單量",
                dataIndex: 'PO_QTY',
                width: '31%',
                renderer: function (val, meta, record) {
                    return record.data['PO_QTY'] + ' ' + record.data['M_PURUN'];
                }
            }, {
                text: "已接收",
                dataIndex: 'PO_ACC_QTY',
                width: '26%',
                renderer: function (val, meta, record) {
                    return record.data['PO_ACC_QTY'];
                }
            },
            //{
            //    text: "接收",
            //    dataIndex: 'ACC_QTY',
            //    align: 'right',
            //    width: '21%',
            //    renderer: function (val, meta, record) {
            //        if (record.data['REPLY_QTY'] > 0) {
            //            if (parseInt(val) != parseInt(record.data['PO_QTY']) - parseInt(record.data['PO_ACC_QTY'])) {
            //                return '<b><font color="red">' + val + '</font></b>'; // REPLY_QTY <> PO_QTY - PO_ACC_QTY
            //            }
            //            else
            //                return val;
            //        }
            //        else
            //            return val;
            //    }
            //},
            {
                text: " ",
                align: 'left',
                flex: 1,
                responsiveConfig: {
                    'width < 600': {
                        hidden: false
                    },
                    'width >= 600': {
                        hidden: true
                    }
                },
                renderer: function (val, meta, record) {
                    return '<a href=\'javascript:popItemDetail("T21");\'><img src="../../../Images/TRA/TRASearch.gif" width="20vh" height="20vh" align="absmiddle" /></a>';
                }
            }],
        listeners: {
            selectionchange: function (model, records) {
                T21Rec = records.length;
                T21LastRec = records[0];

                if (T21LastRec) {
                    T2Query.getForm().findField('T2P0').suspendEvents();
                    T2Query.getForm().findField('T2P0').setValue(T21LastRec.data['MMCODE']);
                    T2Query.getForm().findField('T2P0').resumeEvents();
                }
            },
            cellclick: function (table, td, cellidx, record, tr, rowidx, e, eOpts) {
                if (cellidx == '0' || cellidx == '1' || cellidx == '2') {
                    if (record.data.PO_ACC_QTY == record.data.PO_QTY) { // 已接收=訂單量 
                        Ext.Msg.alert('訊息', '已接收=訂單量,不能再接收');
                        return false;
                    } else {
                        popAccForm('進貨接收處理');
                        msglabel('');
                    }
                }
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        // 院內碼清單(上半部)
        height: '100%',
        layout: 'fit',
        closable: false,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        sortableColumns: false,
        plugins: 'bufferedrenderer',
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }, {
            dock: 'bottom',
            xtype: 'container',
            items: [T21Grid]
        }],
        selModel: "cellmodel",
        plugins: [
            Ext.create("Ext.grid.plugin.CellEditing", {
                clicksToEdit: 1//控制點擊幾下啟動編輯
            })
        ],
        columns: [
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '25%'
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: '30%'
            }, {
                text: "訂單/已收",
                dataIndex: 'M_STOREID',
                width: '23%',
                renderer: function (val, meta, record) {
                    return record.data['PO_QTY'] + '/' + record.data['PO_ACC_QTY'] + ' ' + record.data['M_PURUN'];
                }
            }, {
                text: '<font size="red">接收</font>',
                dataIndex: 'ACC_QTY',
                align: 'left',
                width: '15%',
                editor: {
                    xtype: 'textfield',
                    maskRe: /[0-9]/,
                    regexText: '只能輸入數字',
                    regex: /^(([1-9][0-9]+)|[0-9])$/, // 用正規表示式限制可輸入內容
                    selectOnFocus: true,
                    listeners: {

                    }
                },
                renderer: function (val, meta, record) {
                    if (record.data['REPLY_QTY'] > 0) {
                        if (parseInt(val) != parseInt(record.data['PO_QTY']) - parseInt(record.data['PO_ACC_QTY'])) {
                            return '<b><font color="red">' + val + '</font></b>'; // REPLY_QTY <> PO_QTY - PO_ACC_QTY
                        }
                        else
                            return val;
                    }
                    else
                        return val;
                }
            }, {
                text: " ",
                align: 'left',
                flex: 1,
                responsiveConfig: {
                    'width < 600': {
                        hidden: false
                    },
                    'width >= 600': {
                        hidden: true
                    }
                },
                renderer: function (val, meta, record) {
                    return '<a href=\'javascript:popItemDetail("T2");\'><img src="../../../Images/TRA/TRASearch.gif" width="15vh" height="15vh" align="absmiddle" /></a>';
                }
            }],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                //T2Tool.down('#distConfirmBtn').setDisabled(T2Rec === 0);

                if (T2LastRec) {
                    T2Query.getForm().findField('T2P0').suspendEvents();
                    T2Query.getForm().findField('T2P0').setValue(T2LastRec.data['MMCODE']);
                    T2Query.getForm().findField('T2P0').resumeEvents();
                }
            }
        }
    });

    var M_CONTIDtoMsg = function (m_contid) {
        if (m_contid == '0')
            return '合約';
        else if (m_contid == '2')
            return '非合約';
        else if (m_contid == '3')
            return '小採';
        else
            return m_contid;
    }

    var callableWin = null;
    popItemDetail = function (parTtype) {
        if (!callableWin) {
            Ext.define('T22Model', {
                extend: 'Ext.data.Model',
                fields: ['INID_NAME', 'PO_QTY']
            });
            var lastPoNo, lastMmcode;
            if (parTtype == 'T2') {
                lastPoNo = T2LastRec.data['PO_NO'];
                lastMmcode = T2LastRec.data['MMCODE'];
            }
            else if (parTtype == 'T21') {
                lastPoNo = T21LastRec.data['PO_NO'];
                lastMmcode = T21LastRec.data['MMCODE']
            }

            var T22Store = Ext.create('Ext.data.Store', {
                model: T22Model,
                pageSize: 1000,
                remoteSort: true,
                sorters: [{ property: 'INID_NAME', direction: 'ASC' }],
                proxy: {
                    type: 'ajax',
                    actionMethods: {
                        read: 'POST' // by default GET
                    },
                    url: '/api/CC0002/DetailInid',
                    reader: {
                        type: 'json',
                        rootProperty: 'etts',
                        totalProperty: 'rc'
                    }
                },
                listeners: {
                    beforeload: function (store, options) {
                        var np = {
                            p0: lastPoNo,
                            p1: lastMmcode
                        };
                        Ext.apply(store.proxy.extraParams, np);
                    },
                    load: function (store, records, successful, operation, eOpts) {
                        if (records.length == 0)
                            popMainform.down('#itemdetailgrid').setVisible(false);
                        else
                            popMainform.down('#itemdetailgrid').setVisible(true);
                    }
                }
            });

            var popMainform = Ext.create('Ext.panel.Panel', {
                //id: 'itemDetailCard',
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    // 院內碼明細
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
                                    fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    readOnly: true,
                                    padding: '4 0 16 0'
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '中文品名',
                                    name: 'MMNAME_C',
                                    readOnly: true
                                    //}, {
                                    //    xtype: 'displayfield',
                                    //    fieldLabel: '英文品名',
                                    //    name: 'MMNAME_E',
                                    //    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '包裝單位',
                                    name: 'M_PURUN',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '計量單位',
                                    name: 'BASE_UNIT',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '轉換率',
                                    name: 'UNIT_SWAP',
                                    readOnly: true
                                }, {
                                    xtype: 'grid',
                                    id: 'itemdetailgrid',
                                    store: T22Store,
                                    plain: true,
                                    loadingText: '處理中...',
                                    loadMask: true,
                                    cls: 'T1',
                                    width: '100%',
                                    columns: [
                                        {
                                            text: "單位名稱",
                                            dataIndex: 'INID_NAME',
                                            width: '60%'
                                        }, {
                                            text: "申請數量",
                                            dataIndex: 'PO_QTY',
                                            width: '30%'
                                        }
                                    ]
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

            if (parTtype == 'T2')
                popMainform.down('#itemdetailform').loadRecord(T2LastRec);
            else if (parTtype == 'T21')
                popMainform.down('#itemdetailform').loadRecord(T21LastRec);

            T22Store.load();
        }
        callableWin.show();
    }

    popUdiDetail = function () {
        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                //id: 'itemDetailCard',
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    // UDI明細
                    xtype: 'form',
                    id: 'udidetailform',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 35%',
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
                                    fieldLabel: '公司',
                                    name: 'WmCmpy',
                                    readOnly: true,
                                    padding: '4 0 0 0'
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '倉庫',
                                    name: 'WmWhs',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '成本中心',
                                    name: 'WmOrg',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '供應商料號',
                                    name: 'CrVmpy',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '供應商料號',
                                    name: 'CrItm',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '供應商料號',
                                    name: 'WmRefCode',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '材料盒',
                                    name: 'WmBox',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '儲位',
                                    name: 'WmLoc',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '技術碼',
                                    name: 'WmSrv',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '規格碼',
                                    name: 'WmSku',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '資材碼',
                                    name: 'WmMid',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '院內俗名',
                                    name: 'WmMidName',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '院內品名',
                                    name: 'WmMidNameH',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '規格',
                                    name: 'WmSkuSpec',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '廠牌',
                                    name: 'WmBrand',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '型號',
                                    name: 'WmMdl',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '類別',
                                    name: 'WmMidCtg',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '效期',
                                    name: 'WmEffcDate',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '批號',
                                    name: 'WmLot',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '序號',
                                    name: 'WmSeno',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '包裝名稱',
                                    name: 'WmPak',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '包裝量',
                                    name: 'WmQy',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '本次讀取條碼',
                                    name: 'ThisBarcode',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '單項累計條碼',
                                    name: 'UdiBarcodes',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '單項累計讀取GTIN',
                                    name: 'GtinString',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '健保局條碼',
                                    name: 'NhiBarcode',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '累計健保局條碼',
                                    name: 'NhiBarcodes',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '條碼別',
                                    name: 'BarcodeType',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: 'GtinIn字串',
                                    name: 'GtinInString',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '查詢結果',
                                    name: 'Result',
                                    readOnly: true
                                }, {
                                    xtype: 'displayfield',
                                    fieldLabel: '錯誤訊息',
                                    name: 'ErrMsg',
                                    readOnly: true
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

            callableWin = GetPopWin(viewport, popMainform, 'UDI明細', viewport.width * 0.9, viewport.height * 0.9);

            var f = popMainform.down('#udidetailform').getForm();
            f.findField('WmCmpy').setValue(UdiRec.WmCmpy);
            f.findField('WmWhs').setValue(UdiRec.WmWhs);
            f.findField('WmOrg').setValue(UdiRec.WmOrg);
            f.findField('CrVmpy').setValue(UdiRec.CrVmpy);
            f.findField('CrItm').setValue(UdiRec.CrItm);
            f.findField('WmRefCode').setValue(UdiRec.WmRefCode);
            f.findField('WmBox').setValue(UdiRec.WmBox);
            f.findField('WmLoc').setValue(UdiRec.WmLoc);
            f.findField('WmSrv').setValue(UdiRec.WmSrv);
            f.findField('WmSku').setValue(UdiRec.WmSku);
            f.findField('WmMid').setValue(UdiRec.WmMid);
            f.findField('WmMidName').setValue(UdiRec.WmMidName);
            f.findField('WmMidNameH').setValue(UdiRec.WmMidNameH);
            f.findField('WmSkuSpec').setValue(UdiRec.WmSkuSpec);
            f.findField('WmBrand').setValue(UdiRec.WmBrand);
            f.findField('WmMdl').setValue(UdiRec.WmMdl);
            f.findField('WmMidCtg').setValue(UdiRec.WmMidCtg);
            f.findField('WmEffcDate').setValue(UdiRec.WmEffcDate);
            f.findField('WmLot').setValue(UdiRec.WmLot);
            f.findField('WmSeno').setValue(UdiRec.WmSeno);
            f.findField('WmPak').setValue(UdiRec.WmPak);
            f.findField('WmQy').setValue(UdiRec.WmQy);
            f.findField('ThisBarcode').setValue(UdiRec.ThisBarcode);
            f.findField('UdiBarcodes').setValue(UdiRec.UdiBarcodes);
            f.findField('GtinString').setValue(UdiRec.GtinString);
            f.findField('NhiBarcode').setValue(UdiRec.NhiBarcode);
            f.findField('NhiBarcodes').setValue(UdiRec.NhiBarcodes);
            f.findField('BarcodeType').setValue(UdiRec.BarcodeType);
            f.findField('GtinInString').setValue(UdiRec.GtinInString);
            f.findField('Result').setValue(UdiRec.Result);
            f.findField('ErrMsg').setValue(UdiRec.ErrMsg);
        }
        callableWin.show();
    }

    // 一鍵接收確認
    popAccAllConfirm = function () {
        var store = T2Grid.getStore();
        //var records = store.getUpdatedRecords();//有變動的筆數
        var records = store.getRange(); // 全部筆數
        var data = [];
        var s = '';//single row
        var t = '';//all rows

        Ext.Array.each(records, function (model) {
            data.push(model.data);
        });

        var allValid = false;
        var invalidList = '';
        var checkZeroStr = '';
        if (records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                s = records[i].data.MMCODE + "^" + records[i].data.WEXP_ID + "^"
                    + records[i].data.M_STOREID + "^" + records[i].data.MAT_CLASS + "^" + records[i].data.PO_QTY + "^"
                    + records[i].data.M_PURUN + "^" + records[i].data.UNIT_SWAP + "^" + records[i].data.EXP_DATE + "^"
                    + records[i].data.BW_SQTY + "^" + records[i].data.INQTY + "^" + records[i].data.BASE_UNIT + "^"
                    + records[i].data.ACC_QTY + "^" + records[i].data.PH_REPLY_SEQ + "^" + records[i].data.LOT_NO + "^" + records[i].data.INVOICE;
                if (i == records.length - 1)
                    t += s;
                else
                    t += s + "ˋ";

                if ((parseInt(records[i].data.ACC_QTY) < 0)
                    || (parseInt(records[i].data.ACC_QTY) > parseInt(records[i].data.PO_QTY) - parseInt(records[i].data.PO_ACC_QTY)))
                    invalidList = invalidList + '<font color=red>' + records[i].data.MMCODE + '：接收量' + records[i].data.ACC_QTY + '不合理</font><br>';
                if (parseInt(records[i].data.ACC_QTY) == 0)
                    checkZeroStr = checkZeroStr + records[i].data.MMCODE + '：接收量=0，此品項將不處理<br>';
            }
            if (invalidList == '') {
                invalidList = '*所有院內碼之進貨接收量都合理*<br>' + checkZeroStr;
                allValid = true;
            }
        } else {
            Ext.Msg.alert('提醒', '沒有已填寫的接收量!');
            return false;
        }
        var msgContent = '<p style ="line-height:150%"><font size="3vmin">(1) 接收量 <= 訂單量-已收量。'
            + '<br>(2) 接收量 = 0 品項將不處理。'
            + '<br><br>'
            + invalidList
            + '<br></font></p>';

        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'accallform',
                    height: '100%',
                    layout: 'fit',
                    padding: '4 4 4 4',
                    closable: false,
                    border: false,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 35%',
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
                                    xtype: 'label',
                                    name: 'accAllMsg',
                                    html: msgContent
                                }
                            ]
                        }
                    ]
                }],
                buttons: [{
                    id: 'accAllBtn',
                    itemId: 'accAllBtn',
                    disabled: true,
                    text: '<font size="3vmin">接收確認</font>',
                    height: '6vmin',
                    handler: function () {
                        popMainform.down('#accAllBtn').setDisabled(true);
                        T2UpdateSubmit(t);
                    }
                }, {
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

            callableWin = GetPopWin(viewport, popMainform, '一鍵接收：進貨接收量確認', viewport.width * 0.9, viewport.height * 0.9);

            popMainform.down('#accAllBtn').setDisabled(false);
        }
        callableWin.show();

        if (allValid)
            Ext.ComponentQuery.query('button[itemId=accAllBtn]')[0].setDisabled(false);
        else
            Ext.ComponentQuery.query('button[itemId=accAllBtn]')[0].setDisabled(true);
    }

    // 進貨接收處理(有批號效期)
    popAccForm = function (parTitle) {
        if (!callableWin) {
            var bw_qty_regex;
            if (T21LastRec.data['M_STOREID'] == '1')
                bw_qty_regex = /^0$/; // 庫備
            else
                bw_qty_regex = /^([1-9][0-9]{0,9}|0)$/; // 非庫備

            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'itemqtyform',
                    height: '100%',
                    //layout: 'fit',
                    closable: false,
                    scrollable: true,
                    fieldDefaults: {
                        xtype: 'textfield',
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 40%',
                        width: '100%'
                    },
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'PO_NO_DISP',
                            margin: '4 0 0 8',
                            readOnly: true
                        }, {
                            xtype: 'displayfield',
                            // fieldLabel: '院內碼',
                            name: 'MMCODE_DISP',
                            margin: '0 0 0 8',
                            readOnly: true
                        }, {
                            xtype: 'fieldset',
                            //title: '',
                            autoHeight: true,
                            style: "margin:5px;background-color: #ecf5ff;",
                            cls: 'fieldset-title-bigsize',
                            width: '100%',
                            layout: 'anchor',
                            items: [
                                {
                                    xtype: 'container',
                                    layout: {
                                        type: 'table',
                                        columns: 1
                                    },
                                    items: [
                                        {
                                            xtype: 'displayfield',
                                            // fieldLabel: '中文品名',
                                            name: 'MMNAME_C',
                                            readOnly: true
                                        }, {
                                            xtype: 'displayfield',
                                            // fieldLabel: '庫備',
                                            name: 'M_STOREID',
                                            readOnly: true
                                        }
                                    ]
                                }
                            ]
                        }, {
                            xtype: 'fieldset',
                            id: 'accForm1',
                            autoHeight: true,
                            style: "margin:5px;background-color: #ecf5ff;",
                            cls: 'fieldset-title-bigsize',
                            width: '100%',
                            layout: 'anchor',
                            items: [
                                {
                                    xtype: 'container',
                                    layout: {
                                        type: 'table',
                                        columns: 1
                                    },
                                    items: [
                                        {
                                            xtype: 'hiddenfield',
                                            name: 'PH_REPLY_SEQ1'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'INVOICE1'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'UNIT_SWAP1'
                                        }, {
                                            xtype: 'textfield',
                                            displayField: 'LOT_NO',
                                            valueField: 'LOT_NO',
                                            //id: 'LOT_NO',
                                            name: 'LOT_NO1',
                                            fieldCls: 'required',
                                            //allowBlank: false,
                                            padding: '4vh 0 0 0',
                                            fieldLabel: '批號',
                                            editable: true
                                        }, {
                                            xtype: 'datefield',
                                            fieldLabel: '效期',
                                            name: 'EXP_DATE1',
                                            //id: 'EXP_DATE',
                                            fieldCls: 'required',
                                            //allowBlank: false,
                                            maskRe: /[0-9]/,
                                            regex: /^([0-9]{7})$/,
                                            regexText: '格式需為yyymmdd,例如1080101',
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            field.expand();
                                                        }, 300);
                                                    }
                                                }
                                            }
                                        }, {
                                            //    xtype: 'textfield',
                                            //    fieldLabel: '借貨量',
                                            //    name: 'BW_SQTY1',
                                            //    //id: 'BW_SQTY',
                                            //    maskRe: /[0-9]/,
                                            //    regex: bw_qty_regex,
                                            //    regexText: '最多十位自然數',
                                            //    enforceMaxLength: true,
                                            //    maxLength: 10
                                            //}, {
                                            xtype: 'textfield',
                                            fieldLabel: '交貨量',
                                            name: 'INQTY1',
                                            //id: 'INQTY',
                                            fieldCls: 'required',
                                            maskRe: /[0-9]/,
                                            regex: /^([1-9][0-9]{0,9})$/,
                                            regexText: '最多十位正整數',
                                            enforceMaxLength: true,
                                            maxLength: 10
                                        }, {
                                            xtype: 'textfield',
                                            fieldLabel: '實際接收',
                                            name: 'ACC_QTY1',
                                            //id: 'ACC_QTY',
                                            fieldCls: 'required',
                                            //allowBlank: false,
                                            maskRe: /[0-9]/,
                                            regex: /^([1-9][0-9]{0,9}|0)$/,
                                            regexText: '最多十位整數',
                                            enforceMaxLength: true,
                                            maxLength: 10
                                        }, {
                                            xtype: 'textarea',
                                            fieldLabel: '備註',
                                            id: 'MEMO1',
                                            enforceMaxLength: true,
                                            maxLength: 300
                                        }
                                    ]
                                }
                            ]
                        }, {
                            //原設計最多抓2筆ph_reply lotno data
                            xtype: 'panel',
                            border: false,
                            padding: '0 0 16 0',
                            layout: {
                                type: 'hbox',
                            },
                            items: [{
                                xtype: 'button',
                                text: '取消',
                                width: '50%',
                                margin: '1',
                                handler: function () {
                                    T2Query.getForm().findField('T2P0').setValue('');
                                    T21Store.load();
                                    this.up('window').destroy();
                                    callableWin = null;
                                }
                            }, {
                                xtype: 'button',
                                text: '接收',
                                width: '50%',
                                margin: '1',
                                handler: function () {
                                    var f = popMainform.down('#itemqtyform').getForm();
                                    if (f.isValid()) {
                                        var extraMsg = '';
                                        var tmpACC_QTY1 = f.findField('ACC_QTY1').getValue();
                                        var tmpBW_SQTY1 = 0;
                                        if (tmpACC_QTY1 == '')
                                            tmpACC_QTY1 = 0;
                                        if (tmpBW_SQTY1 == '')
                                            tmpBW_SQTY1 = 0;
                                        if (f.findField('INQTY1').getValue() == '' || f.findField('INQTY1').getValue() == '0') {
                                            Ext.Msg.alert('訊息', '交貨量為必填且不可為0');
                                            return false;
                                        }
                                        if (f.findField('ACC_QTY1').getValue() == '' || f.findField('ACC_QTY1').getValue() == '0') {
                                            Ext.Msg.alert('訊息', '實際接收量為必填且不可為0');
                                            return false;
                                        }
                                        // 批號Grid只有一筆
                                        if (f.findField('LOT_NO1').getValue().trim() == '' || f.findField('EXP_DATE1').getValue() == null)
                                            extraMsg += '<b><font color="red">批號、效期不可空白</font></b></br>';
                                        if (parseInt(tmpACC_QTY1) + parseInt(tmpBW_SQTY1)
                                            > parseInt(T21LastRec.data['ACC_QTY']))
                                            extraMsg += '<b><font color="red">實際接收 > 預計接收</font></b></br>';


                                        var endMsg = '';
                                        var msgBtns;
                                        if (extraMsg == '') {
                                            endMsg = '</br>按[確定]後</br>新增進貨接收紀錄並更新庫存數量';
                                            extraMsg = '<font color="blue">通過檢查</font></br>';
                                            msgBtns = Ext.MessageBox.OKCANCEL;
                                        }
                                        else
                                            msgBtns = Ext.MessageBox.CANCEL;

                                        var confirmMsg = '** 批號、效期檢查不為空白 **</br>'
                                            + '** 實際接收 <= 預計接收 **</br></br>'
                                            + extraMsg + endMsg;

                                        Ext.Msg.show({
                                            title: '實際接收量 確認',
                                            width: '90%',
                                            message: confirmMsg,
                                            buttons: msgBtns,
                                            multiLine: true,
                                            fn: function (btn) {
                                                if (btn === 'ok') {
                                                    T21Submit('1');
                                                    //if (!popMainform.down('#accForm2').hidden)
                                                    //    T21Submit('2');
                                                }
                                            }
                                        });
                                    }
                                    else
                                        Ext.Msg.alert('訊息', '輸入資料格式有誤');
                                }
                            }]
                        }, {
                            xtype: 'hidden',
                            // fieldLabel: '院內碼',
                            name: 'MMCODE',
                            submitValue: true
                        }
                    ]
                }]
            });
            if (T21LastRec.data['PO_QTY'] != T21LastRec.data['PO_ACC_QTY'])
                popMainform.down('#itemqtyform').getForm().findField('ACC_QTY1').setValue(parseInt(T21LastRec.data['PO_QTY']) - parseInt(T21LastRec.data['PO_ACC_QTY']));
            if (T21LastRec.data['INQTY'] == '' || T21LastRec.data['INQTY'] == '0')
                popMainform.down('#itemqtyform').getForm().findField('INQTY1').setValue(T21LastRec.data['PO_QTY']);
            else {
                popMainform.down('#itemqtyform').getForm().findField('INQTY1').setValue(T21LastRec.data['INQTY']);
                popMainform.down('#itemqtyform').getForm().findField('ACC_QTY1').setValue(T21LastRec.data['INQTY']);
            }

            callableWin = GetPopWin(viewport, popMainform, parTitle, viewport.width * 0.9, viewport.height * 0.9);
            //有批號效期
            function T21Submit(fnum) {
                var f = popMainform.down('#itemqtyform').getForm();
                var valLOT_NO;
                var valEXP_DATE;
                var valBW_SQTY;
                var valINQTY;
                var valACC_QTY;
                var valMEMO;
                var valPH_REPLY_SEQ;
                var valINVOICE;
                if (fnum == '1') {
                    valLOT_NO = f.findField('LOT_NO1').getValue();
                    valEXP_DATE = f.findField('EXP_DATE1').getValue();
                    valBW_SQTY = 0;
                    valINQTY = f.findField('INQTY1').getValue();
                    valACC_QTY = f.findField('ACC_QTY1').getValue();
                    valMEMO = f.findField('MEMO1').getValue();
                    valPH_REPLY_SEQ = f.findField('PH_REPLY_SEQ1').getValue();
                    valINVOICE = f.findField('INVOICE1').getValue();
                }
                Ext.Ajax.request({
                    url: accLogSet,
                    params: {
                        po_no: T21LastRec.data['PO_NO'],
                        mmcode: T21LastRec.data['MMCODE'],
                        agen_no: T1LastRec.data['AGEN_NO'],
                        lot_no: valLOT_NO,
                        exp_date: valEXP_DATE,
                        bw_sqty: valBW_SQTY,
                        inqty: valINQTY,
                        acc_qty: valACC_QTY,
                        base_unit: T21LastRec.data['BASE_UNIT'],
                        memo: valMEMO,
                        m_storeid: T21LastRec.data['M_STOREID'],
                        mat_class: T21LastRec.data['MAT_CLASS'],
                        po_qty: T21LastRec.data['PO_QTY'],  //po_qty2
                        m_purun: T21LastRec.data['M_PURUN'],
                        unit_swap: T21LastRec.data['UNIT_SWAP'],
                        wexp_id: T21LastRec.data['WEXP_ID'],
                        PH_REPLY_SEQ: valPH_REPLY_SEQ,
                        invoice: valINVOICE
                    },
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        // 若accForm2隱藏, 則一次submit則切換畫面,否則在accForm2的submit才切換
                        var viewChg = true;
                        if (data.msg == "FULL") {
                            data.msg = "訂單數量已進貨完成，此筆接收失敗!";
                            Ext.Msg.alert('訊息', data.msg);
                            msglabel(data.msg);
                        }
                        else if (data.msg == "DOUBLE") {
                            data.msg = "此筆資料已接收過!";
                            Ext.Msg.alert('訊息', data.msg);
                            msglabel(data.msg);
                        }
                        else {
                            if (viewChg) {
                                if (T21LastRec.data['M_STOREID'] == '1') // 庫備
                                    showItemList();
                                else // 非庫備
                                {
                                    var tmpACC_QTY1 = f.findField('ACC_QTY1').getValue();

                                    if (f.findField('ACC_QTY1').getValue() == '')
                                        tmpACC_QTY1 = 0;
                                    var dispAccQty = parseInt(tmpACC_QTY1) + ' (' + T21LastRec.data['BASE_UNIT'] + ')';
                                    showDistList(dispAccQty);
                                }
                                if (T21LastRec.data['PO_QTY'] == tmpACC_QTY1) {
                                    T3Tool.down('#mmcodeDistBtn').setDisabled(true);
                                    Ext.Msg.alert('訊息', '[進貨接收數量]=[訂單量]，<font color=blue>已自動完成分配</font>。');
                                } else {
                                    Ext.Msg.alert('訊息', '接收數量存檔完成，請進行分配。');
                                }
                                msglabel('接收數量存檔成功');
                                callableWin.destroy();
                                callableWin = null;
                            }
                        }
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('訊息', '進貨接收紀錄新增失敗');
                    }
                });
            }

            justShow = true;

            var f = popMainform.down('#itemqtyform');
            f.loadRecord(T21LastRec);

            var po_no_contid = '';
            if (T1LastRec.data['M_CONTID'] == '0')
                po_no_contid = '(合約)';
            else if (T1LastRec.data['M_CONTID'] == '2')
                po_no_contid = '(非合約)';
            else if (T1LastRec.data['M_CONTID'] == '3')
                po_no_contid = '(小採)';
            f.getForm().findField('PO_NO_DISP').setValue(T21LastRec.data['PO_NO'] + ' ' + po_no_contid);
            f.getForm().findField('MMCODE_DISP').setValue('院內碼 ' + f.getForm().findField('MMCODE').getValue());

            f.getForm().findField('ACC_QTY1').focus();

            var matclassDisp = '';
            if (T21LastRec.data['MAT_CLASS'] == '01')
                matclassDisp = '[藥品]';
            else if (T21LastRec.data['MAT_CLASS'] == '02' || T21LastRec.data['MAT_CLASS'] == '03' ||
                T21LastRec.data['MAT_CLASS'] == '04' || T21LastRec.data['MAT_CLASS'] == '05' ||
                T21LastRec.data['MAT_CLASS'] == '06' || T21LastRec.data['MAT_CLASS'] == '07' ||
                T21LastRec.data['MAT_CLASS'] == '08')
                matclassDisp = '[衛材]';

            // 預計接收量
            //var qtyDisp = '預計接收：' + T21LastRec.data['ACC_QTY'] + ' ' + T21LastRec.data['BASE_UNIT'];
            qtyDisp = '訂單量/已接收：' + T21LastRec.data['PO_QTY'] + '/' + T21LastRec.data['PO_ACC_QTY'] + ' ' + T21LastRec.data['M_PURUN'];
            // 代碼轉庫備/非庫備
            var storeidDisp = '';
            if (T21LastRec.data['M_STOREID'] == '1')
                storeidDisp = '庫備';
            else
                storeidDisp = '非庫備';
            f.getForm().findField('M_STOREID').setValue('<b><font color="blue">' + storeidDisp + '</font></b>' + matclassDisp + ' ' + qtyDisp);

            //// 取得各批號資料
            //function setLotnoData(po_no, mmcode, agen_no) {
            //    lotnoDataStore.removeAll();
            //    Ext.Ajax.request({
            //        url: lotNoComboGet,
            //        params: {
            //            po_no: po_no,
            //            mmcode: mmcode,
            //            agen_no: agen_no
            //        },
            //        method: reqVal_p,
            //        success: function (response) {
            //            var data = Ext.decode(response.responseText);
            //            if (data.success) {
            //                var tb_data = data.etts;
            //                if (tb_data.length > 0) {
            //                    for (var i = 0; i < tb_data.length; i++) {
            //                        var exp_date = tb_data[i].EXP_DATE.toString();
            //                        if (exp_date.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || exp_date.substring(0, 24) == 'Mon Jan 01 0001 00:00:00'
            //                            || exp_date.substring(0, 19) == '0001-01-01T00:00:00') {
            //                            tb_data[i].EXP_DATE = '';  // 若資料庫值為null,在這邊另外做清除
            //                        }
            //                    }

            //                    for (var i = 0; i < tb_data.length; i++) {
            //                        lotnoDataStore.add({
            //                            PO_NO: tb_data[i].PO_NO, MMCODE: tb_data[i].MMCODE, LOT_NO: tb_data[i].LOT_NO,
            //                            EXP_DATE: tb_data[i].EXP_DATE, BW_SQTY: tb_data[i].BW_SQTY, INQTY: tb_data[i].INQTY
            //                        });
            //                    }

            //                    var f = popMainform.down('#itemqtyform');
            //                    f.getForm().findField('LOT_NO1').setValue(tb_data[0].LOT_NO); // 預設代第一筆資料
            //                    f.loadRecord(lotnoDataStore.getAt(0));
            //                    // 進貨接收量預設與交貨量相同
            //                    f.getForm().findField('ACC_QTY1').setValue(tb_data[0].INQTY);
            //                }
            //            }
            //        },
            //        failure: function (response, options) {

            //        }
            //    });
            //}
            //setLotnoData(T21LastRec.data['PO_NO'], T21LastRec.data['MMCODE'], T1LastRec.data['AGEN_NO']);

            // 取得批號資料
            function getLotnoData(po_no, mmcode, agen_no) {
                lotnoDataStore.removeAll();
                Ext.Ajax.request({
                    url: lotNoDataGet,
                    params: {
                        po_no: po_no,
                        mmcode: mmcode,
                        agen_no: agen_no
                    },
                    method: reqVal_p,
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            var tb_data = data.etts;
                            if (tb_data.length > 0) {
                                for (var i = 0; i < tb_data.length; i++) {
                                    var exp_date = tb_data[i].EXP_DATE.toString();
                                    if (exp_date.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || exp_date.substring(0, 24) == 'Mon Jan 01 0001 00:00:00'
                                        || exp_date.substring(0, 19) == '0001-01-01T00:00:00') {
                                        tb_data[i].EXP_DATE = '';  // 若資料庫值為null,在這邊另外做清除
                                    }
                                    else
                                        tb_data[i].EXP_DATE = exp_date.substring(0, 10);
                                }
                                var f = popMainform.down('#itemqtyform');
                                f.getForm().findField('LOT_NO1').setValue(tb_data[0].LOT_NO);
                                f.getForm().findField('EXP_DATE1').setValue(tb_data[0].EXP_DATE);
                                f.getForm().findField('INQTY1').setValue(tb_data[0].INQTY);
                                f.getForm().findField('ACC_QTY1').setValue(tb_data[0].INQTY); // 實際接收量預設與交貨量相同
                                f.getForm().findField('PH_REPLY_SEQ1').setValue(tb_data[0].PH_REPLY_SEQ);
                                f.getForm().findField('INVOICE1').setValue(tb_data[0].INVOICE);
                            }
                        }
                    },
                    failure: function (response, options) {

                    }
                });
            }
            getLotnoData(T21LastRec.data['PO_NO'], T21LastRec.data['MMCODE'], T1LastRec.data['AGEN_NO']);

        }
        callableWin.show();
    }

    // 依掃描條碼接收單一品項
    popScanDistForm = function (parTitle, loadData, barcode) {
        var distFormFieldCls = '';
        var distFormAllowBlank = true;
        var distFormHidden = true;
        if (loadData.WEXP_ID == 'Y') {
            // 若為批號效期管制品項,批號和效期必填
            distFormFieldCls = 'required';
            distFormAllowBlank = false;
            distFormHidden = false;
        }
        //進貨數量=0不啟用[進貨接收確認]
        var disabledtButton = false;
        if (loadData.INQTY == '0') {
            disabledtButton = true;
        }
        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'ScanDistForm',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 45%',
                        width: '97%'
                    },
                    items: [
                        {
                            xtype: 'container',
                            layout: 'vbox',
                            padding: '2vmin',
                            scrollable: true,
                            items: [
                                {
                                    xtype: 'container',
                                    width: '100%',
                                    layout: {
                                        type: 'table',
                                        columns: 1
                                    },
                                    items: [
                                        {
                                            xtype: 'displayfield',
                                            fieldLabel: '條碼',
                                            name: 'BARCODE'
                                            // 原條碼為textfield,可掃描其他品項切換,但實際需求只在此顯示上層掃到的條碼即可
                                            //,listeners: {
                                            //    change: function (field, nValue, oValue, eOpts) {
                                            //        if (nValue.length >= 8) {
                                            //            var newBarcode = nValue;
                                            //            field.setValue('');
                                            //            // 先關閉目前視窗,重新填入條碼後依新條碼查詢品項
                                            //            this.up('window').destroy();
                                            //            callableWin = null;
                                            //            T1Query.getForm().findField('P2').setValue('');
                                            //            T1Query.getForm().findField('P2').focus();
                                            //            chkBarcode(newBarcode);
                                            //        }
                                            //    }
                                            //}
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '訂單編號',
                                            name: 'PO_NO'
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '品名',
                                            name: 'MMNAME_C'
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '包裝單位',
                                            name: 'M_PURUN'
                                        }, {
                                            xtype: 'textfield',
                                            name: 'LOT_NO',
                                            fieldCls: distFormFieldCls,
                                            allowBlank: distFormAllowBlank,
                                            hidden: distFormHidden,
                                            fieldLabel: '批號'
                                        }, {
                                            xtype: 'datefield',
                                            fieldLabel: '效期',
                                            name: 'EXP_DATE',
                                            maskRe: /[0-9]/,
                                            regex: /^([0-9]{7})$/,
                                            regexText: '格式需為yyymmdd,例如1080101',
                                            fieldCls: distFormFieldCls,
                                            allowBlank: distFormAllowBlank,
                                            hidden: distFormHidden,
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            field.expand();
                                                        }, 300);
                                                    }
                                                }
                                            }
                                        },
                                        //{
                                        //    xtype: 'textfield',
                                        //    fieldLabel: '訂單數量',
                                        //    name: 'PO_QTY',
                                        //    maskRe: /[0-9]/,
                                        //    regex: /^([1-9][0-9]{0,9})$/,
                                        //    regexText: '最多十位正整數',
                                        //    enforceMaxLength: true,
                                        //    maxLength: 10,
                                        //    readonly: true
                                        //},
                                        {
                                            xtype: 'displayfield',
                                            fieldLabel: '訂單數量',
                                            name: 'PO_QTY',
                                            readonly: true
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '中央庫房自留數量/已分配',
                                            name: 'RETAIN_QTY'
                                        }, {
                                            xtype: 'textfield',
                                            fieldLabel: '進貨數量',
                                            name: 'INQTY',
                                            fieldCls: 'required',
                                            allowBlank: false,
                                            maskRe: /[0-9]/,
                                            regex: /^([1-9][0-9]{0,9})|0$/,
                                            regexText: '最多十位整數',
                                            enforceMaxLength: true,
                                            maxLength: 10,
                                            selectOnFocus: true,
                                            enableKeyEvents: true,
                                            listeners: {
                                                keydown: function (field, e, eOpts) {
                                                    if (e.keyCode == '13')
                                                        popMainform.down('#ScanDistConfirm').click();
                                                }
                                            }
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'STOREID'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'PH_REPLY_SEQ'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'INVOICE'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'UNIT_SWAP'
                                        }, {
                                            xtype: 'hiddenfield',
                                            name: 'REPLY_QTY'
                                        }
                                    ]
                                }, {
                                    xtype: 'panel',
                                    width: '100%',
                                    border: false,
                                    layout: {
                                        type: 'hbox',
                                    },
                                    items: [{
                                        xtype: 'button',
                                        text: '取消',
                                        width: '50%',
                                        margin: '1',
                                        handler: function () {
                                            this.up('window').destroy();
                                            callableWin = null;
                                            T1Query.getForm().findField('P2').setValue('');
                                            T1Query.getForm().findField('P2').focus();
                                        }
                                    }, {
                                        xtype: 'button',
                                        itemId: 'ScanDistConfirm',
                                        id: 'ScanDistConfirm',
                                        text: '進貨接收確認',
                                        width: '50%',
                                        margin: '1',
                                        disabled: disabledtButton,
                                        handler: function () {
                                            console.log(new Date());
                                            Ext.getCmp('ScanDistConfirm').disable();
                                            var f = popMainform.down('#ScanDistForm').getForm();
                                            if (f.isValid()) {
                                                var po_qty = f.findField('PO_QTY').getValue(); // 訂單數量
                                                var inqty = f.findField('INQTY').getValue(); // 進貨數量
                                                var reply_qty = f.findField('REPLY_QTY').getValue();//廠商回覆數量
                                                
                                                if (reply_qty != null && reply_qty != '') {
                                                    // 有回復數量，進貨量不可超過回復量
                                                    if (parseInt(reply_qty) < parseInt(inqty)) {
                                                        Ext.Msg.alert('訊息', '[進貨數量]不可大於[廠商回覆數量]');
                                                        Ext.getCmp('ScanDistConfirm').enable();
                                                        return;
                                                    }
                                                }
                                                if (parseInt(inqty) <= parseInt(po_qty)) {
                                                    if (parseInt(inqty) > 0) {

                                                        T4Submit();
                                                    }
                                                    else {
                                                        Ext.getCmp('ScanDistConfirm').enable();
                                                        Ext.Msg.alert('訊息', '[進貨數量]需大於0');
                                                    }

                                                }
                                                else {
                                                    Ext.getCmp('ScanDistConfirm').enable();
                                                    Ext.Msg.alert('訊息', '[進貨數量]不可大於[訂單數量]');
                                                }

                                            }
                                            else {
                                                Ext.getCmp('ScanDistConfirm').enable();
                                                Ext.Msg.alert('訊息', '輸入資料格式有誤');
                                            }
                                        }
                                    }]
                                }, {
                                    xtype: 'hidden',
                                    // fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    submitValue: true
                                }, {
                                    xtype: 'grid',
                                    id: 'scanDistDetail',
                                    width: '100%',
                                    //layout: 'fit',
                                    closable: false,
                                    store: T3Store,
                                    plain: true,
                                    loadingText: '處理中...',
                                    loadMask: true,
                                    plugins: 'bufferedrenderer',
                                    cls: 'T1',
                                    columns: [
                                        {
                                            text: "申請單位",
                                            dataIndex: 'INID_NAME',
                                            width: '45%'
                                        }, {
                                            text: "申請量/已分配",
                                            dataIndex: 'PR_QTY',
                                            width: '35%',
                                            renderer: function (val, meta, record) {
                                                return record.data.PR_QTY + "/" + record.data.DSUM_QTY;
                                            }
                                        }, {
                                            text: "分配量",
                                            labelAlign: 'left',
                                            dataIndex: 'DIST_QTY',
                                            align: 'left',
                                            width: '25%',
                                            editor: {
                                                xtype: 'textfield',
                                                maskRe: /[0-9]/,
                                                regexText: '只能輸入數字',
                                                regex: /^([1-9][0-9]*|0)$/,
                                                selectOnFocus: true
                                            }
                                        }, {
                                            text: " ",
                                            align: 'left',
                                            flex: 1
                                        }],
                                    plugins: [
                                        Ext.create('Ext.grid.plugin.CellEditing', {
                                            clicksToEdit: 1,//控制點擊幾下啟動編輯
                                            listeners: {
                                                edit: function (editor, context, eOpts) {

                                                }
                                            }
                                        })
                                    ],
                                    listeners: {
                                        selectionchange: function (model, records) {
                                            T3Rec = records.length;
                                            T3LastRec = records[0];
                                            //T3Tool.down('#mmcodeDistBtn').setDisabled(T3Rec === 0); // 由原本逐項分配改為填完各筆分配量後一起分配
                                        }
                                    }
                                }
                            ]
                        }
                    ]
                }]
            });

            function T4Submit() {
                setTimeout(function () {
                    // 計算分配量總和
                    var distQtySum = 0;
                    var chkMSG = "";
                    var SCAN_ACC_QTY = parseInt(popMainform.down('#ScanDistForm').getForm().findField('INQTY').getValue());
                    var retain_qty = parseInt(loadData.RETAIN_QTY);
                    var retain_distqty = parseInt(loadData.RETAIN_DISTQTY);

                    for (var i = 0; i < T3Store.data.items.length; i++) {
                        var itemDistQty = 0;
                        if (T3Store.data.items[i].data.DIST_QTY) {
                            itemDistQty = T3Store.data.items[i].data.DIST_QTY;
                            if ((parseInt(T3Store.data.items[i].data.PR_QTY) - parseInt(T3Store.data.items[i].data.DSUM_QTY)) < parseInt(T3Store.data.items[i].data.DIST_QTY))
                                chkMSG += T3Store.data.items[i].data.INID_NAME + "->[分配量]超過[申請量]-[已分配];</br>";
                        }
                        distQtySum += parseInt(itemDistQty);
                    }
                    //loadData.UNIT_SWAP  轉換率== "1"才可以分配
                    // 2022-05-20: 取消僅判斷轉換率=1的非庫備品項，有轉換率仍需判斷
                    //if (loadData.UNIT_SWAP == "1" && loadData.STOREID == "0" && chkMSG != "")
                    if (loadData.STOREID == "0" && chkMSG != "") {
                        Ext.getCmp('ScanDistConfirm').enable();
                        Ext.Msg.alert('訊息', chkMSG);
                    }
                    //else if (loadData.UNIT_SWAP == "1" && loadData.STOREID == "0" && T3Store.data.items.length != 0 && distQtySum == 0)
                    //else if (loadData.STOREID == "0" && T3Store.data.items.length != 0 && distQtySum == 0) {
                    //    Ext.getCmp('ScanDistConfirm').enable();
                    //    Ext.Msg.alert('訊息', '[分配量總和]不可等於 0');
                    //}
                    //else if (loadData.UNIT_SWAP == "1" && loadData.STOREID == "0" && distQtySum != SCAN_ACC_QTY)
                    //2023-03-16: 配合庫房縮減註解下方判斷
                    //else if (loadData.STOREID == "0" && distQtySum != SCAN_ACC_QTY) {
                    //    Ext.getCmp('ScanDistConfirm').enable();
                    //    Ext.Msg.alert('訊息', '[分配量總和]=' + distQtySum + ' 不等於[進貨數量]=' + SCAN_ACC_QTY);
                    //}
                    //2023-03-16: 配合庫房縮減註解下方判斷 分配量總和不可大於進貨量
                    else if (loadData.STOREID == "0" && distQtySum > SCAN_ACC_QTY) {
                        Ext.getCmp('ScanDistConfirm').enable();
                        Ext.Msg.alert('訊息', '[分配量總和]=' + distQtySum + ' 大於[進貨數量]=' + SCAN_ACC_QTY);
                    }
                    //2023-04-06: 配合庫房縮減增加下方判斷   有庫房自留數量檢查是否有超量
                    else if (loadData.STOREID == "0" && (retain_qty > 0) && ((SCAN_ACC_QTY - distQtySum) > (retain_qty - retain_distqty))) {
                        Ext.getCmp('ScanDistConfirm').enable();
                        Ext.Msg.alert('訊息', '[進貨數量-分配量總和]=' + (SCAN_ACC_QTY - distQtySum) + '不可大於 [庫房自留數量-庫房已自留數量]=' + (retain_qty - retain_distqty));
                    }
                    //2023-04-06: 配合庫房縮減增加下方判斷  無庫房自留數量檢查是否等於進貨數量
                    else if (loadData.STOREID == "0" && (retain_qty == 0) && (distQtySum != SCAN_ACC_QTY)) {
                        Ext.getCmp('ScanDistConfirm').enable();
                        Ext.Msg.alert('訊息', '[分配量總和]=' + distQtySum + ' 不等於[進貨數量]=' + SCAN_ACC_QTY);
                    }
                    else {
                        var conDistQty = '';
                        //var conSeq = ''; // 這個階段SEQ還沒建立
                        var conInid = '';
                        for (var i = 0; i < T3Store.data.items.length; i++) {
                            if (i == T3Store.data.items.length - 1) {
                                // 最後一筆資料不加分割符號
                                conDistQty = conDistQty + T3Store.data.items[i].data.DIST_QTY;
                                //conSeq = conSeq + T3Store.data.items[i].data.SEQ;
                                conInid = conInid + T3Store.data.items[i].data.INID;
                            }
                            else {
                                // 若尚有資料則後面加上^分割符號
                                conDistQty = conDistQty + T3Store.data.items[i].data.DIST_QTY + '^';
                                //conSeq = conSeq + T3Store.data.items[i].data.SEQ + '^';
                                conInid = conInid + T3Store.data.items[i].data.INID + '^';
                            }

                        }

                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        var myMask1 = new Ext.LoadMask(popMainform, { msg: '處理中...' });
                        myMask.show();
                        myMask1.show();
                        var f = popMainform.down('#ScanDistForm').getForm();
                        Ext.Ajax.request({
                            url: scanDistLogSet,
                            timeout:60000,
                            params: {
                                po_no: f.findField('PO_NO').getValue(),
                                mmcode: f.findField('MMCODE').getValue(),
                                lot_no: f.findField('LOT_NO').getValue(),
                                exp_date: f.findField('EXP_DATE').getValue(),
                                po_qty: f.findField('PO_QTY').getValue(),
                                inqty: f.findField('INQTY').getValue(),
                                agen_no: loadData.AGEN_NO,
                                unit_swap: loadData.UNIT_SWAP,
                                m_purun: loadData.M_PURUN,
                                base_unit: loadData.BASE_UNIT,
                                m_storeid: loadData.STOREID,
                                mat_class: loadData.MAT_CLASS,
                                wexp_id: loadData.WEXP_ID,
                                dist_qty: conDistQty,
                                inid: conInid,
                                ph_reply_seq: loadData.PH_REPLY_SEQ,
                                invoice: loadData.INVOICE,
                                acc_qty: f.findField('INQTY').getValue()
                            },
                            method: reqVal_p,
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                myMask.hide();
                                myMask1.hide();
                                if (data.msg == '接收數量存檔..成功') {
                                    msglabel(loadData.MMCODE + '接收成功');
                                    callableWin.destroy();
                                    callableWin = null;
                                    T1Query.getForm().findField('P2').setValue('');
                                    T1Query.getForm().findField('P2').focus();
                                    
                                    //  Ext.getCmp('ScanDistConfirm').enable();
                                } else {
                                    if (data.msg == "FULL")
                                        data.msg = "訂單數量已進貨完成，此筆接收失敗!";
                                    else if (data.msg == "DOUBLE")
                                        data.msg = "此筆資料已接收過!";
                                    Ext.Msg.alert('訊息', data.msg);
                                    msglabel(data.msg);
                                    
                                    // Ext.getCmp('ScanDistConfirm').enable();
                                }
                            },
                            failure: function (response, options) {
                                myMask.hide();
                                myMask1.hide();
                            }
                        });
                    }
                }, 300);
            }
            T3Store.getProxy().url = '/api/CC0002/DistAll_SCAN';
            T3Store.load({
                params: {
                    p0: loadData.PO_NO,
                    p1: loadData.MMCODE
                }
            });
            callableWin = GetPopWin(viewport, popMainform, parTitle, viewport.width * 0.9, viewport.height * 0.9);
            var f = popMainform.down('#ScanDistForm');
            var v_exp_date = loadData.EXP_DATE;
            if (v_exp_date.substring(0, 21) == 'Mon Jan 01 1 00:00:00' || v_exp_date.substring(0, 24) == 'Mon Jan 01 0001 00:00:00'
                || v_exp_date.substring(0, 19) == '0001-01-01T00:00:00') {
                loadData.EXP_DATE = '';
            }
            else
                loadData.EXP_DATE = v_exp_date.substring(0, 10);
            f.getForm().findField('BARCODE').setValue(barcode);
            f.getForm().findField('PO_NO').setValue(loadData.PO_NO);
            f.getForm().findField('MMNAME_C').setValue(loadData.MMNAME_C);
            f.getForm().findField('M_PURUN').setValue(loadData.M_PURUN);
            f.getForm().findField('LOT_NO').setValue(loadData.LOT_NO);
            f.getForm().findField('EXP_DATE').setValue(loadData.EXP_DATE);
            f.getForm().findField('PO_QTY').setValue(loadData.PO_QTY);
            f.getForm().findField('RETAIN_QTY').setValue(loadData.RETAIN_QTY + "/" + loadData.RETAIN_DISTQTY);
            if (loadData.INQTY == 0) {
                f.getForm().findField('INQTY').setValue('已進貨完成!');
            } else {
                f.getForm().findField('INQTY').setValue(loadData.INQTY);
            }

            f.getForm().findField('MMCODE').setValue(loadData.MMCODE);
            f.getForm().findField('STOREID').setValue(loadData.STOREID);
            f.getForm().findField('PH_REPLY_SEQ').setValue(loadData.PH_REPLY_SEQ);
            f.getForm().findField('INVOICE').setValue(loadData.INVOICE);
        }
        //非庫備才顯示 分配GRID
        if (popMainform.down('#ScanDistForm').getForm().findField('STOREID').getValue() == '0')
            popMainform.down('#scanDistDetail').setVisible(true);
        else
            popMainform.down('#scanDistDetail').setVisible(false);

        callableWin.show();
        f.getForm().findField('INQTY').focus();
        if (loadData.UNIT_SWAP != "1") {
            Ext.Msg.alert('訊息', '轉換率=' + loadData.UNIT_SWAP + '不等於 1，接收後請至AA0006手動分配。');
        }
    }

    // 單位非庫備分配
    popDistForm = function (parTitle) {
        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'distForm',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    border: true,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 35%',
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
                                    xtype: 'container',
                                    width: '100%',
                                    layout: {
                                        type: 'table',
                                        columns: 1
                                    },
                                    items: [
                                        {
                                            xtype: 'displayfield',
                                            fieldLabel: '申請單位',
                                            name: 'INID_NAME'
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '中文品名',
                                            name: 'MMNAME_C'
                                        }, {
                                            xtype: 'displayfield',
                                            fieldLabel: '申請量',
                                            name: 'PR_QTY'
                                        }, {
                                            xtype: 'combo',
                                            store: lotnoDataStore,
                                            displayField: 'LOT_NO',
                                            valueField: 'LOT_NO',
                                            name: 'LOT_NO',
                                            padding: '4vh 0 0 0',
                                            //fieldCls: 'required',
                                            //allowBlank: false,
                                            fieldLabel: '批號',
                                            queryMode: 'local',
                                            autoSelect: true,
                                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            if (justShow == false)
                                                                field.expand();
                                                            else
                                                                justShow = false;
                                                        }, 300);
                                                    }
                                                },
                                                select: function (field, record, eOpts) {
                                                    var f = popMainform.down('#distForm');
                                                    f.loadRecord(lotnoDataStore.findRecord('LOT_NO', record.get('LOT_NO')));
                                                    // 分配量預設為申請量-借貨量
                                                    var cultDistQty = parseInt(f.getForm().findField('PR_QTY').getValue().split(' ')[0]) - 0;
                                                    f.getForm().findField('DIST_QTY').setValue(cultDistQty);
                                                }
                                            }
                                        }, {
                                            xtype: 'datefield',
                                            fieldLabel: '效期',
                                            name: 'EXP_DATE',
                                            maskRe: /[0-9]/,
                                            regex: /^([0-9]{7})$/,
                                            regexText: '格式需為yyymmdd,例如1080101',
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            field.expand();
                                                        }, 300);
                                                    }
                                                }
                                            }
                                        }, {
                                            //    xtype: 'textfield',
                                            //    fieldLabel: '借貨量',
                                            //    name: 'BW_SQTY',
                                            //    fieldCls: 'required',
                                            //    allowBlank: false,
                                            //    maskRe: /[0-9]/,
                                            //    regex: /^([1-9][0-9]{0,9}|0)$/,
                                            //    regexText: '最多十位自然數',
                                            //    enforceMaxLength: true,
                                            //    maxLength: 10
                                            //}, {
                                            xtype: 'textfield',
                                            fieldLabel: '分配量',
                                            name: 'DIST_QTY',
                                            fieldCls: 'required',
                                            allowBlank: false,
                                            maskRe: /[0-9]/,
                                            regex: /^([1-9][0-9]{0,9})$/,
                                            regexText: '最多十位正整數',
                                            enforceMaxLength: true,
                                            maxLength: 10
                                        }, {
                                            xtype: 'textarea',
                                            fieldLabel: '備註',
                                            name: 'MEMO',
                                            enforceMaxLength: true,
                                            maxLength: 300
                                        }
                                    ]
                                }, {
                                    xtype: 'hidden',
                                    // fieldLabel: '訂單號碼',
                                    name: 'PO_NO',
                                    submitValue: true
                                }, {
                                    xtype: 'hidden',
                                    // fieldLabel: '院內碼',
                                    name: 'MMCODE',
                                    submitValue: true
                                }, {
                                    xtype: 'hidden',
                                    name: 'SEQ',
                                    submitValue: true
                                }, {
                                    xtype: 'hidden',
                                    name: 'PR_DEPT',
                                    submitValue: true
                                }

                            ]
                        }, {
                            xtype: 'panel',
                            width: '100%',
                            border: false,
                            layout: {
                                type: 'hbox',
                            },
                            items: [{
                                xtype: 'button',
                                text: '取消',
                                width: '50%',
                                margin: '1',
                                handler: function () {
                                    this.up('window').destroy();
                                    callableWin = null;
                                }
                            }, {
                                xtype: 'button',
                                text: '分配確認',
                                width: '50%',
                                margin: '1',
                                handler: function () {
                                    var f = popMainform.down('#distForm').getForm();
                                    if (f.isValid()) {
                                        var pr_qty = f.findField('PR_QTY').getValue().split(' ')[0]; // 申請量
                                        var bw_sqty = 0; // 借貨量
                                        var dist_qty = f.findField('DIST_QTY').getValue(); // 分配量
                                        if (parseInt(dist_qty) <= parseInt(pr_qty) - parseInt(bw_sqty)) {
                                            Ext.MessageBox.confirm('分配量確認', '是否確認分配', function (btn, text) {
                                                if (btn === 'yes') {
                                                    T3Submit();
                                                }
                                            });
                                        }
                                        else
                                            Ext.Msg.alert('訊息', '分配量需小於等於申請量-借貨量');

                                    }
                                    else
                                        Ext.Msg.alert('訊息', '輸入資料格式有誤');
                                }
                            }]
                        }
                    ]
                }]
            });

            function T3Submit() {
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();
                var f = popMainform.down('#distForm').getForm();
                T3Store.getProxy().url = '/api/CC0002/DistAll';
                Ext.Ajax.request({
                    url: distLogSet,
                    params: {
                        po_no: T21LastRec.data['PO_NO'],
                        mmcode: T21LastRec.data['MMCODE'],
                        seq: f.findField('SEQ').getValue(),
                        pr_dept: f.findField('PR_DEPT').getValue(),
                        bw_sqty: f.findField('BW_SQTY').getValue(),
                        lot_no: f.findField('LOT_NO').getValue(),
                        exp_date: f.findField('EXP_DATE').getValue(),
                        dist_qty: f.findField('DIST_QTY').getValue(),
                        memo: f.findField('MEMO').getValue()
                    },
                    method: reqVal_p,
                    success: function (response) {
                        myMask.hide();
                        T3Store.load();
                        // getHadDistQty();
                        msglabel('非庫備分配紀錄新增成功');
                        callableWin.destroy();
                        callableWin = null;
                    },
                    failure: function (response, options) {
                        myMask.hide();
                    }
                });
            }

            callableWin = GetPopWin(viewport, popMainform, parTitle, viewport.width * 0.9, viewport.height * 0.9);

            var f = popMainform.down('#distForm');
            f.loadRecord(T3LastRec);

            f.getForm().findField('LOT_NO').setValue('');

            // 以  0 (單位) 方式呈現
            var qtyDisp = f.getForm().findField('PR_QTY').getValue() + ' (' + T21LastRec.data['BASE_UNIT'] + ')';
            f.getForm().findField('PR_QTY').setValue(qtyDisp);

            // 代入借貨量和分配量預設值
            //if (f.getForm().findField('BW_SQTY').getValue() == null || f.getForm().findField('BW_SQTY').getValue() == '')
            //    f.getForm().findField('BW_SQTY').setValue('0');

            if (f.getForm().findField('DIST_QTY').getValue() == null || f.getForm().findField('DIST_QTY').getValue() == '') {
                //if (f.getForm().findField('BW_SQTY').getValue())
                //    f.getForm().findField('DIST_QTY').setValue(parseInt(f.getForm().findField('PR_QTY').getValue()) - parseInt(f.getForm().findField('BW_SQTY').getValue()));
                //else
                f.getForm().findField('DIST_QTY').setValue(f.getForm().findField('PR_QTY').getValue());
            }
        }
        callableWin.show();
    }

    // 檢查廠商編號
    function chkAgenno() {
        Ext.Ajax.request({
            url: chkAgennoGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var AgenN = data.msg;
                    if (AgenN != '') {
                        viewport.down('#agenform').setTitle(AgenN);
                        //viewport.down('#form').collapse();
                        // T1Grid.down('#rtnQueryBtn').setDisabled(false);
                        // T1Grid.down('#poConfirmBtn').setDisabled(true);
                        T1Load();
                    }
                    else {
                        Ext.Msg.alert('訊息', '廠商編號不存在。');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function T2UpdateSubmit(gridData) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: accAllSet,
            params: {
                po_no: T1LastRec.data['PO_NO'],
                agen_no: T1LastRec.data['AGEN_NO'],
                gridData: gridData
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    // T2Store.load();
                    isACC = true;
                    T2Tool.down('#distConfirmBtn').setDisabled(true);
                    msglabel('一鍵接收完成');
                    callableWin.destroy();
                    callableWin = null;
                    alert(data.msg);
                }
            },
            failure: function (response, options) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                Ext.Msg.alert('訊息', data.msg);
            }
        });
    }

    // 切換至訂單清單
    showPoList = function () {
        var layout = viewport.down('#t1Grid').getLayout();
        layout.setActiveItem(0);
    }

    // 切換至院內碼清單
    showItemList = function () {
        isACC = false;
        var layout = viewport.down('#t1Grid').getLayout();
        layout.setActiveItem(1);
        T2Query.getForm().findField('T2P0').setValue('');
        T2Query.getForm().findField('T2P0').focus();
        T2Store.load();
        T21Store.load();
    }

    // 切換到非庫備品申請明細
    showDistList = function (dispAccQty) {
        var layout = viewport.down('#t1Grid').getLayout();
        layout.setActiveItem(2);

        T3Form.loadRecord(T21LastRec);
        // 進貨接收
        T3Form.getForm().findField('ACC_QTY').setValue(dispAccQty);

        // 取得已分配量
        // getHadDistQty();
        // 取得非庫備品申請明細
        T3Store.load();
    }

    // 主畫面查詢:檢查條碼
    function chkBarcode(barcode) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: chkBarcodeGet,
            params: {
                p0: barcode,
                p1: T1Query.getForm().findField('P1').getRawValue(),
                P11: T1Query.getForm().findField('P11').getRawValue()
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    if (rtnStr == 'bcnotfound') {
                        Ext.Msg.alert('訊息', '條碼編號目前查<font color=red>無對應資料</font>');
                        T1Query.getForm().findField('P2').setValue('');
                    }
                    else if (rtnStr == 'ponotfound') {
                        Ext.Msg.alert('訊息', T1Query.getForm().findField('P1').getRawValue() + '~' + T1Query.getForm().findField('P11').getRawValue() + '<font color=red>沒有訂單及進貨記錄</font>');
                        T1Query.getForm().findField('P2').setValue('');
                    }
                    else {
                        var tb_data = data.etts;
                        if (tb_data) {
                            if (tb_data.length > 0) {
                                PoRec = tb_data[0];
                                //T1Store.load({
                                //    params: {
                                //        p0: '',
                                //        p1: '',
                                //        p2: PoRec.PO_NO
                                //    }
                                //});
                                popScanDistForm('進貨接收', PoRec, barcode);
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    // 訂單品項明細:檢查院內碼
    function chkMmcode(mmcode) {
        Ext.Ajax.request({
            url: chkMmcodeGet,
            params: {
                p0: T2Query.getForm().findField('T2P0').getValue(),
                p1: T1LastRec.data['PO_NO']
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    if (rtnStr == 'bcnotfound') {
                        Ext.Msg.alert('訊息', '掃描值不存在或已停用');
                    }
                    else if (rtnStr == 'ponotfound') {
                        Ext.Msg.alert('訊息', '不在進貨接收清單中');
                    }
                    else {
                        T2Query.getForm().findField('T2P0').suspendEvents(false);
                        T2Query.getForm().findField('T2P0').setValue(rtnStr);
                        T2Query.getForm().findField('T2P0').resumeEvents(true);
                        T2Rec = 1;
                        T2LastRec = T2Store.findRecord('MMCODE', rtnStr);
                        //showItemQty();
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 取得已分配量
    //function getHadDistQty() {
    //    Ext.Ajax.request({
    //        url: hadDistQtyGet,
    //        params: {
    //            po_no: T21LastRec.data['PO_NO'],
    //            mmcode: T21LastRec.data['MMCODE'],
    //            seq: LastSeq
    //        },
    //        method: reqVal_p,
    //        success: function (response) {
    //            var data = Ext.decode(response.responseText);
    //            if (data.success) {
    //                //T3Form.getForm().findField('HAD_DIST_QTY').setValue(data.msg);
    //            }
    //        },
    //        failure: function (response, options) {

    //        }
    //    });
    //}

    // 取得UDI資料(主畫面)
    function T1getUdiData(barcode) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: udiDataGet,
            params: {
                p0: barcode
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                var tb_data = data.etts;
                if (tb_data) {
                    if (tb_data.length > 0) {
                        UdiRec = tb_data[0];
                        chkBarcode(UdiRec.WmMid); // UDI查詢有資料則代入MMCODE查詢
                    }
                    else
                        chkBarcode(barcode);
                }
                else
                    chkBarcode(barcode);
            },
            failure: function (response, options) {
                myMask.hide();
                chkBarcode(barcode);
            }
        });
    }

    // 取得UDI資料(第二層畫面)
    function getUdiData() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: udiDataGet,
            params: {
                p0: T2Query.getForm().findField('T2P0').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                var tb_data = data.etts;
                if (tb_data) {
                    if (tb_data.length > 0) {
                        UdiRec = tb_data[0];
                        popUdiDetail();
                        msglabel(data.msg);
                        T2Query.getForm().findField('T2P0').setValue(UdiRec.WmMid);
                    }
                    else {
                        chkMmcode(T2Query.getForm().findField('T2P0').getValue());
                        msglabel('UDI資訊接收失敗');
                    }
                }
                else {
                    chkMmcode(T2Query.getForm().findField('T2P0').getValue());
                    msglabel('UDI資訊接收失敗');
                }
            },
            failure: function (response, options) {
                myMask.hide();
                chkMmcode(T2Query.getForm().findField('T2P0').getValue());
                msglabel('UDI資訊接收失敗');
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
            //id: 'itemDetailCard',
            //itemId: 't1Grid',
            id: 't1Grid',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid, {
                xtype: 'container',
                layout: 'fit',
                collapsible: false,
                split: true,
                flex: 1,
                items: [
                    T2Grid
                ]
            }, {
                    // 非庫備品申請明細
                    xtype: 'grid',
                    id: 'distDetail',
                    height: '100%',
                    layout: 'fit',
                    closable: false,
                    store: T3Store,
                    plain: true,
                    loadingText: '處理中...',
                    loadMask: true,
                    plugins: 'bufferedrenderer',
                    cls: 'T1',
                    dockedItems: [{
                        dock: 'top',
                        xtype: 'toolbar',
                        items: [T3Form]
                    }, {
                        dock: 'top',
                        xtype: 'toolbar',
                        items: [T3Tool]
                    }],
                    columns: [
                        {
                            text: "申請單位",
                            dataIndex: 'INID_NAME',
                            width: '45%'
                        }, {
                            text: "申請量/已分配",
                            dataIndex: 'PR_QTY',
                            width: '35%',
                            renderer: function (val, meta, record) {
                                return record.data.PR_QTY + "/" + record.data.DSUM_QTY;
                            },
                        }
                        //, {
                        //    text: "借貨量",
                        //    dataIndex: 'BW_SQTY',
                        //    align: 'right',
                        //    width: '20%',
                        //    renderer: function (val, meta, record) {
                        //        if (val == null || val == '')
                        //            return '0';
                        //        else
                        //            return val;
                        //    }
                        //}
                        , {
                            text: "分配量",
                            dataIndex: 'DIST_QTY',
                            align: 'left',
                            width: '25%',
                            editor: {
                                xtype: 'textfield',
                                maskRe: /[0-9]/,
                                regexText: '只能輸入數字',
                                regex: /^([1-9][0-9]*|0)$/,
                                selectOnFocus: true
                            }
                        }, {
                            text: " ",
                            align: 'left',
                            flex: 1
                        }],
                    plugins: [
                        Ext.create('Ext.grid.plugin.CellEditing', {
                            clicksToEdit: 1,//控制點擊幾下啟動編輯
                            listeners: {
                                edit: function (editor, context, eOpts) {

                                }
                            }
                        })
                    ],
                    listeners: {
                        selectionchange: function (model, records) {
                            T3Rec = records.length;
                            T3LastRec = records[0];
                            //T3Tool.down('#mmcodeDistBtn').setDisabled(T3Rec === 0); // 由原本逐項分配改為填完各筆分配量後一起分配
                        }
                    }
                }]
        }]
    });

    // T1Query.getForm().findField('P0').focus();
    Ext.getDoc().dom.title = T1Name;

    T1Query.getForm().findField('P1').setValue(Ext.util.Format.date(Ext.Date.add(new Date(), Ext.Date.MONTH, -1), "Y-m-") + "01");
    // T1Query.getForm().findField('P2').setValue(Ext.Date.getLastDateOfMonth(Ext.Date.add(new Date(), Ext.Date.MONTH, -1)));
    T1Query.getForm().findField('P2').focus();
});
