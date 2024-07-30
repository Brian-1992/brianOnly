Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "申請人員點收";
    var T11Get = '/api/AB0091/DetailAll';
    var T2Get = '/api/AB0091/WexpAll';
    var ackCntGet = '/api/AB0091/GetAckCnt';
    var ackQtySet = '/api/AB0091/SetAckQty';
    var tmpAckQtySet = '/api/AB0091/SetTmpAckQty';
    var docCompleteSet = '/api/AB0091/SetDocComplete';
    var chkMmcodeGet = '/api/AB0091/ChkMmcode';
    var chkScannerGet = '/api/AB0091/ChkScanner';
    var chkUserWhnoGet = '/api/AB0091/GetChkUserWhno';
    var whnoComboGet = '/api/AB0091/GetWH_NoComboSimple';
    var docnoComboGet = '/api/AB0091/GetDocnoCombo';
    var bcWhpickChk = '/api/AB0091/chkBcWhpick';
    var syncAckqty = '/api/AB0091/ackqtySync';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1LastDoc = null; // 記錄最後一次查詢的申請單號,以避免在某些時機重新查詢前,申請單號已被使用者清除或選取其他申請單
    var docnoActNo = 0; // 目前在處理T11Store的第幾筆
    var selType = 0; // 0:品項選擇模式, 1:申請單選擇模式
    var gACKTIMES = 1; // 點收倍率設為全域,當掃完條碼有切換品項時,新品項的數量也會套用點收倍率

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T11Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'SEQ', 'APPDEPT', 'MMCODE', 'MMNAME_C', 'MMNAME_E', 'PICK_QTY', 'ACKQTY', 'BASE_UNIT', 'WEXP_ID', 'WEXP_ID_DESC', 'ACKID', 'POSTID', 'FLOWID', 'BARCODE', 'TRATIO']
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['LOT_NO', 'VALID_DATE', 'ACT_PICK_QTY']
    });

    // 申請單號
    var docnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 部門
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 取得部門清單
    function setWhnoData() {
        Ext.Ajax.request({
            url: whnoComboGet,
            method: reqVal_p,
            success: function (response) {
                whnoQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            whnoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                    else
                        Ext.Msg.alert('訊息', '點收人員尚無對應部門資料');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWhnoData();

    //var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
    //    name: 'P0',
    //    id: 'P0',
    //    fieldLabel: '部門號碼',
    //    fieldCls: 'required',
    //    allowBlank: false,
    //    storeAutoLoad: true,
    //    margin: '1 0 1 0',
    //    limit: 20,
    //    queryUrl: '/api/AB0091/GetWH_NoCombo',
    //    extraFields: ['WH_NO', 'WH_NAME'],
    //    getDefaultParams: function () {
    //    },
    //    listeners: {
    //        select: function (c, r, i, e) {
    //            var f = T1Query.getForm();
    //            if (r.get('WH_NAME') !== '') {
    //                f.findField("P1").setValue(r.get('WH_NAME'));
    //                setDocnoData(r.get('WH_NO'));
    //            }
    //        },
    //        focus: function (field, event, eOpts) {
    //            if (!field.isExpanded) {
    //                setTimeout(function () {
    //                    field.expand();
    //                }, 300);
    //            }
    //        }
    //    }
    //});

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
            labelStyle: 'width: 35%',
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
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    id: 'P0',
                    name: 'P0',
                    fieldCls: 'required',
                    allowBlank: false,
                    margin: '1 0 1 0',
                    fieldLabel: '部門',
                    queryMode: 'local',
                    labelStyle: 'width: 25%',
                    autoSelect: true,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        select: function (c, r, i, e) {
                            if (r.data['VALUE'] !== '') {
                                setDocnoData(r.data['VALUE']);
                            }
                        },
                        focus: function (field, event, eOpts) {
                            if (!field.isExpanded) {
                                setTimeout(function () {
                                    field.expand();
                                }, 300);
                            }
                        }
                    }
                },
                // wh_NoCombo, {
                //    xtype: 'displayfield',
                //    fieldLabel: '部門名稱',
                //    name: 'P1',
                //    id: 'P1',
                //    margin: '1 0 1 0',
                //    enforceMaxLength: true
                //},
                {
                    xtype: 'displayfield',
                    fieldLabel: '點收人員',
                    name: 'P4',
                    id: 'P4',
                    margin: '1 0 1 0',
                    value: session['UserName']
                }, {
                    xtype: 'combo',
                    store: docnoQueryStore,
                    displayField: 'VALUE',
                    valueField: 'VALUE',
                    id: 'P2',
                    name: 'P2',
                    margin: '1 0 1 0',
                    fieldLabel: '申請單號',
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
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '條碼',
                    name: 'P3',
                    id: 'P3',
                    margin: '1 0 1 0',
                    enforceMaxLength: true,
                    //maxLength: 13,
                    labelStyle: 'width: 25%',
                    selectOnFocus: true,
                    listeners: {
                        change: function (field, nValue, oValue, eOpts) {
                            if (nValue.length >= 8) {
                                chkMmcode(nValue);
                            }
                        }
                    }
                }, {
                    xtype: 'panel',
                    border: false,
                    plugins: 'responsive',
                    margin: '1 0 1 0',
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
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            setTimeout(function () {
                                var f = T1Query.getForm();
                                if (!f.isValid())
                                    Ext.Msg.alert('提醒', '沒有指定部門，無法查詢');
                                else {
                                    var p2Value = f.findField('P2').getValue();
                                    var p3Value = f.findField('P3').getValue();
                                    if ((p2Value == '' || p2Value == null) && (p3Value == '' || p3Value == null)) {
                                        Ext.Msg.alert('提醒', '申請單號及條碼至少需填入一個');
                                    }
                                    else {
                                        // 申請單號未填,條碼有填
                                        if ((p2Value == '' || p2Value == null) && p3Value != '' && p3Value != null) {
                                            selType = 1;
                                            T1Grid.down('#COL_DOCNO').setVisible(true);
                                            T1Grid.down('#COL_BASE_UNIT').setVisible(false);
                                        }

                                        // Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                        T1Load();
                                    }
                                }
                            }, 500);

                            msglabel('');
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        height: '9vmin',
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                width: '50%'
                            }
                        },
                        margin: '1',
                        handler: function () {
                            var f = this.up('form').getForm();
                            // f.reset();
                            f.findField('P2').setValue('');
                            f.findField('P3').setValue('');
                            f.findField('P3').focus();
                            msglabel('');
                        }
                    }]
                }, {
                    xtype: 'button',
                    itemId: 'btnDocComp',
                    text: '整張申請單點收完成',
                    height: '9vmin',
                    margin: '1 0 1 0',
                    disabled: true,
                    handler: function () {
                        getAckCnt(T1Query.getForm().findField('P2').getValue(), 'N');
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AB.AB0091', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    T1Store.removeAll();
                    T1Tool.onLoad();
                }
                else {
                    if (records.length > 0) {
                        if (T1Query.getForm().findField('P2').getValue()) // 申請單號有選
                        {
                            T1Query.down('#btnDocComp').setDisabled(false);
                            T1LastRec = records[0]; // 不論資料有幾筆,T1LastRec先設為第一筆,以供'整張申請單點收完成'使用
                            selType = 0;
                            T1Grid.down('#COL_DOCNO').setVisible(false);
                            T1Grid.down('#COL_BASE_UNIT').setVisible(true);
                        }
                        else {
                            if (selType == 1) {
                                // 若查詢申請單只有一筆,直接代入查詢條件並重新查詢
                                if (T1Store.data.length == 1) {
                                    T1LastRec = records[0];
                                    T1Query.getForm().findField('P2').setValue(T1LastRec.data['DOCNO']);
                                    T1Query.down('#btnDocComp').setDisabled(false);
                                    selType = 0;
                                    T1Grid.down('#COL_DOCNO').setVisible(false);
                                    T1Grid.down('#COL_BASE_UNIT').setVisible(true);
                                    T1Load();
                                }
                                else if (T1Store.data.length == 0) {
                                    selType = 0;
                                    T1Grid.down('#COL_DOCNO').setVisible(false);
                                    T1Grid.down('#COL_BASE_UNIT').setVisible(true);
                                }
                                T1Query.getForm().findField('P3').setValue('');
                            }
                            else {
                                T1Query.down('#btnDocComp').setDisabled(true);
                            }
                        }

                        if (records.length == 1 && selType == 0) {
                            if (records[0].data['POSTID'] == 'C' || records[0].data['FLOWID'] == '4') {
                                T1LastRec = records[0];
                                // popItemDetail("U");
                            }
                            else {
                                T1Query.down('#btnDocComp').setDisabled(true);
                            }
                        }
                    }
                    else
                        T1Query.down('#btnDocComp').setDisabled(true);
                }
            }
        }
    });
    var T11Store = Ext.create('Ext.data.Store', {
        model: 'T11Model',
        pageSize: 1000,
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['DOCNO'],
                    p1: T1LastRec.data['MMCODE'],
                    p2: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                var findIdx = store.find('MMCODE', T1LastRec.data['MMCODE']);
                if (findIdx < 0)
                    findIdx = 0;
                docnoActNo = findIdx;

                var pForm = Ext.ComponentQuery.query('panel[id=itemDetail]')[0];
                if (pForm) {
                    pForm.getForm().findField('PAGENUM').setValue((docnoActNo + 1) + '/' + T11Store.data.items.length);
                    pForm.loadRecord(T11Store.data.items[docnoActNo]);

                    // 2021-01-22修改：預帶待收(核撥)數量 = 點收數量
                    // 2021-03-18修改：復原為原邏輯
                    if ((pForm.getForm().findField('MMCODE').getValue() == pForm.getForm().findField('BARCODE').getValue())
                        || pForm.getForm().findField('BARCODE').getValue() == null
                        || pForm.getForm().findField('BARCODE').getValue() == '') {
                        // barcode跟mmcode相同

                        pForm.getForm().findField('ACKQTY').setReadOnly(false);
                        pForm.getForm().findField('ADJQTY').setVisible(true);
                        pForm.down('#tempsubmit').setVisible(true);
                        // 核撥量=點收量時,disable暫存鈕,只能取消或按確定完成點收
                        // 避免使用者點收已達核撥量卻只有暫存,導致之後又刷條碼判斷為尚未點收過的品項
                        if (pForm.getForm().findField('ACKQTY').getValue() == pForm.getForm().findField('PICK_QTY').getValue())
                            pForm.down('#tempsubmit').setDisabled(true);

                        pForm.getForm().findField('ADJQTY').setValue(0);
                        if (!(pForm.getForm().findField('ACKQTY').getValue() > 0))
                            pForm.getForm().findField('ACKQTY').setValue(pForm.getForm().findField('PICK_QTY').getValue());
                    }
                    else {
                        // barcode跟mmcode不同
                        // ACKQTY = PICK_QTY則顯示TRATIO
                        pForm.getForm().findField('ACKQTY').setReadOnly(true);
                        pForm.getForm().findField('ADJQTY').setVisible(false);
                        pForm.down('#tempsubmit').setVisible(false);

                        var tratioVal = parseInt(pForm.getForm().findField('TRATIO').getValue()) * parseInt(gACKTIMES);

                        // 以異動量更新資料庫的值,再取得最新的值更新畫面(因應可能有多人同時點同品項)
                        Ext.Ajax.request({
                            url: syncAckqty,
                            params: {
                                docno: T1LastRec.data['DOCNO'],
                                mmcode: T11Store.data.items[docnoActNo].data['MMCODE'],
                                adjqty: tratioVal
                            },
                            method: reqVal_p,
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    var rtnStr = data.msg;

                                    pForm.getForm().findField('ACKQTY').setValue(rtnStr);

                                    var pick_remain = parseInt(pForm.getForm().findField('PICK_QTY').getValue()) - parseInt(pForm.getForm().findField('ACKQTY').getValue());
                                    if (pick_remain < 0)
                                        pick_remain = 0;
                                    msglabel(pForm.getForm().findField('MMCODE').getValue() + '點收+' + tratioVal + ',尚需' + pick_remain);

                                    // 數量加完後重置點收倍率
                                    gACKTIMES = 1;
                                    pForm.getForm().findField('ACKTIMES').setValue('1');

                                    // 若點收數量已跟待收(核撥)數量相同,儲存資料
                                    if (pForm.getForm().findField('ACKQTY').getValue() == pForm.getForm().findField('PICK_QTY').getValue()) {
                                        setTimeout(function () {
                                            updateQty(pForm.getForm().findField('ACKQTY').getValue(), 'U');
                                        }, 200);
                                    }
                                }
                            },
                            failure: function (response, options) {

                            }
                        });
                    }


                    // 若院內碼筆數大於1,顯示換頁鈕
                    if (T11Store.data.items.length > 1) {
                        pForm.down('#pageButtons').setVisible(true);
                        if (docnoActNo + 1 == 1)
                            pForm.down('#btnPageUp').setDisabled(true);
                        else
                            pForm.down('#btnPageUp').setDisabled(false);
                        if (docnoActNo + 1 == T11Store.data.items.length)
                            pForm.down('#btnPageDown').setDisabled(true);
                        else
                            pForm.down('#btnPageDown').setDisabled(false);
                    }
                    else
                        pForm.down('#pageButtons').setVisible(false);

                    T2Store.removeAll();
                    if (T1LastRec.data['WEXP_ID'] == 'Y') {
                        // 批號效期管理
                        T2Load();
                        pForm.down('#validGrid').setVisible(true);
                    }
                    else
                        pForm.down('#validGrid').setVisible(false);

                    getAckCnt(T1LastRec.data['DOCNO'], 'Y');

                    pForm.getForm().findField('SCANNER').focus();
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T11Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 200,
        remoteSort: true,
        sorters: [{ property: 'VALID_DATE', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['DOCNO'],
                    p1: T1LastRec.data['SEQ']
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {

            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: T2Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });


    function T1Load() {
        T1LastDoc = T1Query.getForm().findField('P2').getValue();
        T1Tool.moveFirst();
    }
    function T11Load() {
        T11Store.load({
            params: {
                start: 0
            }
        });
    }
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    var setWinActWidth;
    var setWinActHeight;
    var GetPopWinMobile = function (viewport, popform, strTitle, winActWidth, winActHeight) {
        setWinActWidth = winActWidth;
        setWinActHeight = winActHeight;
        var win = Ext.widget('window', {
            title: strTitle,
            id: 'win',
            modal: true,
            layout: 'fit',
            autoScroll: true,
            closeAction: 'destroy',
            constrain: true,
            resizable: true,
            closable: false,
            width: winActWidth,
            height: winActHeight,
            items: popform,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                    //xwin.suspendEvent('move');
                    //xwin.center();
                    //xwin.resumeEvent('move');
                },
                resize: function (xwin, width, height) {
                    //winActWidth = setWinActWidth;
                    //winActHeight = setWinActHeight;
                    //xwin.setWidth(setWinActWidth);
                    //xwin.setHeight(setWinActHeight);
                }
            }
        });
        return win;
    };

    Ext.on('resize', function () {
        if (Ext.getCmp('win')) {
            Ext.getCmp('win').setWidth(setWinActWidth);
            Ext.getCmp('win').setHeight(setWinActHeight);
        }
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
        }],
        columns: {
            defaults: {
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                }
            },
            items: [{
                xtype: 'rownumberer'
            }, {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                itemId: 'COL_DOCNO',
                hidden: true,
                width: '35%',
                plugins: '',
                renderer: function (val, meta, record) {
                    return '<font size="2">' + val + '</font>';
                }
            }, {
                text: "項次",
                dataIndex: 'SEQ',
                width: '20%'
            }, {
                text: "申請單位",
                dataIndex: 'APPDEPT',
                width: '25%'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '30%',
                plugins: ''
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: '25%'
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '40%'
            }, {
                text: "核撥量",
                dataIndex: 'PICK_QTY',
                align: 'right',
                width: '15%'
            }, {
                text: "點收量",
                dataIndex: 'ACKQTY',
                align: 'right',
                width: '20%',
                plugins: ''
            }, {
                text: "撥補單位",
                dataIndex: 'BASE_UNIT',
                itemId: 'COL_BASE_UNIT',
                width: '23%',
                plugins: ''
            }, {
                text: "批號效期",
                dataIndex: 'WEXP_ID',
                width: '15%'
            }, {
                text: " ",
                align: 'left',
                width: '10%',
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        flex: 1
                    },
                    'width >= 600': {
                        flex: 0
                    }
                },
                renderer: function (val, meta, record) {
                    //if (record.data['ACKID'])
                    //    return '<a href=\'javascript:popItemDetail("D");\'>取消</a>';
                    //else
                    //    return '<a href=\'javascript:popItemDetail("U");\'>點收</a>';
                    if (record.data['POSTID'] == 'C' || record.data['FLOWID'] == '4')
                        return '<a href=\'javascript:popItemDetail("U");\'>點收</a>';
                }
            }, {
                text: " ",
                align: 'left',
                flex: 1
            }]
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (selType == 1) {
                    if (T1LastRec) {
                        T1Query.getForm().findField('P2').setValue(T1LastRec.data['DOCNO']);
                        T1Query.getForm().findField('P3').setValue('');
                    }
                    selType = 0;
                    T1Grid.down('#COL_DOCNO').setVisible(false);
                    T1Grid.down('#COL_BASE_UNIT').setVisible(true);
                    T1Load();
                }
            }
        }
    });

    var callableWin = null;
    popItemDetail = function (setType) {
        //msglabel('');
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'itemDetail',
                height: '100%',
                //layout: 'fit',
                closable: false,
                scrollable: true,
                border: true,
                fieldDefaults: {
                    labelAlign: 'right',
                    labelWidth: false,
                    labelStyle: 'width: 40%',
                    width: '95%'
                },
                items: [
                    {
                        xtype: 'container',
                        layout: 'hbox',
                        padding: '4 2 4 2',
                        itemId: 'pageButtons',
                        items: [
                            {
                                xtype: 'button',
                                text: '上一個',
                                itemId: 'btnPageUp',
                                labelAlign: 'center',
                                disabled: true,
                                handler: function () {
                                    docnoActNo = docnoActNo - 1;
                                    popform.getForm().findField('PAGENUM').setValue((docnoActNo + 1) + '/' + T11Store.data.items.length);
                                    if (docnoActNo + 1 == 1)
                                        popform.down('#btnPageUp').setDisabled(true);
                                    else
                                        popform.down('#btnPageUp').setDisabled(false);
                                    popform.down('#btnPageDown').setDisabled(false);

                                    popform.loadRecord(T11Store.data.items[docnoActNo]);
                                }
                            }, {
                                xtype: 'displayfield',
                                name: 'PAGENUM',
                                width: null,
                                readOnly: true,
                                padding: '0 2 0 2',
                                value: ''
                            }, {
                                xtype: 'button',
                                text: '下一個',
                                itemId: 'btnPageDown',
                                labelAlign: 'center',
                                handler: function () {
                                    docnoActNo = docnoActNo + 1;
                                    popform.getForm().findField('PAGENUM').setValue((docnoActNo + 1) + '/' + T11Store.data.items.length);
                                    if (docnoActNo + 1 == T11Store.data.items.length)
                                        popform.down('#btnPageDown').setDisabled(true);
                                    else
                                        popform.down('#btnPageDown').setDisabled(false);
                                    popform.down('#btnPageUp').setDisabled(false);

                                    popform.loadRecord(T11Store.data.items[docnoActNo]);
                                }
                            }, {
                                xtype: 'displayfield',
                                name: 'CMPNUM',
                                width: null,
                                readOnly: true,
                                padding: '0 2 0 4',
                                value: '<font color = "blue">已點收 0 項</font>'
                            }
                        ]
                    }, {
                        xtype: 'container',
                        layout: 'vbox',
                        padding: '2vmin',
                        scrollable: true,
                        items: [
                            {
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
                                fieldLabel: '核撥數量',
                                name: 'PICK_QTY',
                                readOnly: true
                            }, {
                                xtype: 'container',
                                layout: 'hbox',
                                width: '95%',
                                items: [
                                    {
                                        xtype: 'textfield',
                                        fieldLabel: '點收數量',
                                        name: 'ACKQTY',
                                        readOnly: false,
                                        selectOnFocus: true,
                                        allowBlank: false,
                                        labelWidth: false,
                                        labelStyle: 'width: 65%',
                                        width: '63%',
                                        maskRe: /[0-9.]/,
                                        regexText: '只能輸入數字',
                                        regex: /^0|[1-9]\d*(\.\d*)?$/,
                                        listeners: {
                                            change: function (field, eOpts) {
                                                popform.getForm().findField('ADJQTY').setValue('0');
                                                if (popform.getForm().findField('PICK_QTY').getValue() == popform.getForm().findField('ACKQTY').getValue())
                                                    popform.down('#tempsubmit').setDisabled(true);
                                                else
                                                    popform.down('#tempsubmit').setDisabled(false);
                                            }
                                        }
                                    }, {
                                        xtype: 'textfield',
                                        fieldLabel: '+',
                                        labelSeparator: '',
                                        name: 'ADJQTY',
                                        readOnly: false,
                                        selectOnFocus: true,
                                        allowBlank: false,
                                        labelWidth: false,
                                        labelStyle: 'width: 30%',
                                        width: '32%',
                                        maskRe: /[0-9.]/,
                                        regexText: '只能輸入數字',
                                        regex: /^0|[1-9]\d*(\.\d*)?$/,
                                        listeners: {
                                            change: function (field, eOpts) {
                                                if (parseInt(popform.getForm().findField('ADJQTY').getValue()) > 0)
                                                    popform.down('#tempsubmit').setDisabled(false);
                                                else
                                                    popform.down('#tempsubmit').setDisabled(true);
                                            }
                                        }
                                    }
                                ]
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '撥補單位',
                                name: 'BASE_UNIT',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '效期註記',
                                name: 'WEXP_ID_DESC',
                                readOnly: true
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '儲位',
                                name: 'STORE_LOC',
                                readOnly: true
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '條碼',
                                name: 'SCANNER',
                                readOnly: false,
                                selectOnFocus: true,
                                listeners: {
                                    change: function (field, nValue, oValue, eOpts) {
                                        if (nValue.length >= 8) {
                                            chkScanner(nValue);
                                        }
                                    }
                                }
                            }, {
                                xtype: 'textfield',
                                fieldLabel: '點收倍率',
                                name: 'ACKTIMES',
                                readOnly: false,
                                selectOnFocus: true,
                                maskRe: /[0-9]/,
                                regexText: '只能輸入數字',
                                regex: /^[1-9]\d*$/
                            }, {
                                xtype: 'hidden',
                                name: 'BARCODE'
                            }, {
                                xtype: 'hidden',
                                name: 'TRATIO'
                            }
                        ]
                    }, {
                        xtype: 'container',
                        padding: '2vmin',
                        itemId: 'validGrid',
                        items: [
                            {
                                xtype: 'grid',
                                store: T2Store,
                                itemId: 'T2Grid',
                                sortableColumns: false,
                                colspan: 2,
                                width: '90%',
                                height: '250',
                                scrollable: true,
                                columns: [
                                    {
                                        text: "批號",
                                        dataIndex: 'LOT_NO',
                                        align: 'left',
                                        style: 'text-align:center',
                                        width: '25%'
                                    }, {
                                        text: "有效日期",
                                        dataIndex: 'VALID_DATE',
                                        align: 'center',
                                        style: 'text-align:center',
                                        width: '40%'
                                    }, {
                                        text: "數量",
                                        dataIndex: 'ACT_PICK_QTY',
                                        align: 'right',
                                        style: 'text-align:center',
                                        width: '30%'
                                    }
                                ]
                            }
                        ]
                    }
                ],
                buttons: [{
                    id: 'confirmsubmit',
                    disabled: false,
                    text: '<font size="3vmin">確定</font>',
                    height: '9vmin',
                    handler: function () {
                        if (popform.getForm().isValid()) {
                            var ackQty = popform.getForm().findField('ACKQTY').getValue();
                            var pickQty = popform.getForm().findField('PICK_QTY').getValue();

                            if (ackQty > 0 && ackQty != pickQty) {
                                Ext.MessageBox.confirm('提醒', '<b><font color="blue">點收量[' + ackQty + ']</font></b>與<b><font color="blue">核撥量[' + pickQty + ']</font></b>不同，</br>是否確定要品項結案?', function (btn, text) {
                                    if (btn === 'yes') {
                                        updateQty(ackQty, 'U');
                                    }
                                });
                            }
                            else
                                updateQty(ackQty, 'U');
                        }
                        else
                            Ext.Msg.alert('訊息', '輸入資料格式有誤');
                        msglabel('');
                    }
                }, {
                    id: 'tempsubmit',
                    itemId: 'tempsubmit',
                    disabled: false,
                    text: '<font size="3vmin">暫存</font>',
                    height: '9vmin',
                    handler: function () {
                        if (popform.getForm().isValid()) {
                            if (parseInt(popform.getForm().findField('ADJQTY').getValue()) > 0) {
                                Ext.MessageBox.confirm('訊息', '是否確定本次點收增加' + popform.getForm().findField('ADJQTY').getValue() + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        updateTmpAdjqty(popform.getForm().findField('ADJQTY').getValue(), '', '', true);
                                        T1Load();
                                    }
                                });
                            }
                            else {
                                Ext.MessageBox.confirm('訊息', '是否確定儲存點收總量' + popform.getForm().findField('ACKQTY').getValue() + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        updateTmpQty(popform.getForm().findField('ACKQTY').getValue(), '', '', true);
                                        T1Load();
                                    }
                                });
                            }
                        }
                        else
                            Ext.Msg.alert('訊息', '輸入資料格式有誤');
                    }
                }, {
                    id: 'winclosed',
                    disabled: false,
                    text: '<font size="3vmin">取消</font>',
                    height: '9vmin',
                    handler: function () {
                        //T1Load();
                        this.up('window').destroy();
                        callableWin = null;
                        msglabel('');
                        T1Query.getForm().findField('P3').setValue('');
                        T1Query.getForm().findField('P3').focus();
                        T1Load();
                    }
                }]
            });

            callableWin = GetPopWinMobile(viewport, popform, '點收內容確認', viewport.width * 0.95, viewport.height * 0.97);
        }
        callableWin.show();

        // 彈出明細視窗時,若已點收過則提示是否要清除之前的點收量
        //if (setType == 'D') {
        //    Ext.MessageBox.confirm('提醒', '此品項已點收，</br>是否確定要刪除點收?', function (btn, text) {
        //        if (btn === 'yes') {
        //            updateQty(0, 'D');
        //        }
        //    });
        //}

        var pForm = Ext.ComponentQuery.query('panel[id=itemDetail]')[0];
        if (pForm)
        {
            if (gACKTIMES > 1)
                pForm.getForm().findField('ACKTIMES').setValue(gACKTIMES);
            else
                pForm.getForm().findField('ACKTIMES').setValue('1');
        }
            
        T11Load(); // 後續載入Form等動作於load事件處理
    }

    // 更新或刪除點收量
    function updateQty(ackQty, setType) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: ackQtySet,
            params: {
                docno: T1LastRec.data['DOCNO'],
                mmcode: T11Store.data.items[docnoActNo].data['MMCODE'],
                ackqty: ackQty,
                setType: setType
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                if (T11Store.data.items.length == 1) {
                    // 資料更新後重載並關閉視窗(等待getAckCnt,若已點=品項數則先清除申請單號)
                    setTimeout(function () {
                        T1Load();
                    }, 500);

                    callableWin.destroy();
                    callableWin = null;
                }
                if (setType == 'U') {
                    getAckCnt(T1LastRec.data['DOCNO'], 'Y');
                    msglabel('院內碼' + T11Store.data.items[docnoActNo].data['MMCODE'] + '點收完成');
                    // T1Query.getForm().findField('P2').setValue('');
                    T1Query.getForm().findField('P3').setValue('');
                    T1Query.getForm().findField('P3').focus();
                }
                else if (setType == 'D')
                    getAckCnt(T1LastRec.data['DOCNO'], 'Y');

            },
            failure: function (response, options) {
                myMask.hide();
                callableWin.destroy();
                callableWin = null;
                msglabel('點收失敗');
            }
        });
    }

    // 暫存點收量(總量) -> 總量=核撥量時不允許暫存,以免之後掃描條碼被視為未點過的品項
    function updateTmpQty(ackqty, newMmcode, newBarcode, closeDetail) {
        Ext.Ajax.request({
            url: tmpAckQtySet,
            params: {
                docno: T1LastRec.data['DOCNO'],
                mmcode: T11Store.data.items[docnoActNo].data['MMCODE'],
                ackqty: ackqty
            },
            method: reqVal_p,
            success: function (response) {
                msglabel(T11Store.data.items[docnoActNo].data['MMCODE'] + '點收量已暫存');
                if (T11Store.data.items.length == 1) {
                    if (closeDetail) {
                        callableWin.destroy();
                        callableWin = null;
                        T11Load();
                        T1Query.getForm().findField('P3').setValue('');
                        T1Query.getForm().findField('P3').setValue(newBarcode);
                    }
                    else
                        T1Load();
                }
                getAckCnt(T1LastRec.data['DOCNO'], 'Y');

            },
            failure: function (response, options) {

            }
        });
    }
    // 暫存點收量(異動量)
    function updateTmpAdjqty(adjqty, newMmcode, newBarcode, closeDetail) {
        Ext.Ajax.request({
            // url: tmpAckQtySet,
            url: syncAckqty,
            params: {
                docno: T1LastRec.data['DOCNO'],
                mmcode: T11Store.data.items[docnoActNo].data['MMCODE'],
                adjqty: adjqty
            },
            method: reqVal_p,
            success: function (response) {
                msglabel(T11Store.data.items[docnoActNo].data['MMCODE'] + '點收量已暫存');

                var data = Ext.decode(response.responseText);
                var rtnStr = data.msg;

                // 以異動量更新後,若等於核撥量則完成點收
                if (rtnStr == T11Store.data.items[docnoActNo].data['PICK_QTY'])
                {
                    updateQty(rtnStr, 'U');
                }
                else
                {
                    if (T11Store.data.items.length == 1) {
                        if (closeDetail) {
                            callableWin.destroy();
                            callableWin = null;
                            T11Load();
                            T1Query.getForm().findField('P3').setValue('');
                            T1Query.getForm().findField('P3').setValue(newBarcode);
                        }
                        else
                            T1Load();
                    }
                    getAckCnt(T1LastRec.data['DOCNO'], 'Y');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 取得申請單號已點收或未點收數(Y:已點收,N:未點收)
    function getAckCnt(docno, getType) {
        Ext.Ajax.request({
            url: ackCntGet,
            params: {
                docno: T1LastRec.data['DOCNO'],
                getType: getType
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (getType == 'Y') {
                    var pForm = Ext.ComponentQuery.query('panel[id=itemDetail]')[0];
                    if (pForm)
                        pForm.getForm().findField('CMPNUM').setValue('<font color = "blue">已點收 ' + data.msg + ' 項</font>');
                    if (data.msg == T1Store.getCount())
                        setDocnoData(T1Query.getForm().findField('P0').getValue()); // 若已點收數等於申請單品項數

                }
                else if (getType == 'N') {
                    if (data.msg == '0') {
                        Ext.MessageBox.confirm('訊息', '將設定此申請單點收完成', function (btn, text) {
                            if (btn === 'yes') {
                                docComplete(T1LastRec.data['DOCNO'], '0');
                            }
                        });
                    }
                    else {
                        Ext.MessageBox.confirm('訊息', '此申請單尚有' + data.msg + '個品項未點收完成</br>，是否確定要將未點收品項自動點收，</br>並將申請單結案?', function (btn, text) {
                            if (btn === 'yes') {
                                docComplete(T1LastRec.data['DOCNO'], '1');
                            }
                        });
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
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });

    // 取得申請單號清單
    function setDocnoData(wh_no) {
        Ext.Ajax.request({
            url: docnoComboGet,
            params: {
                wh_no: wh_no
            },
            method: reqVal_p,
            success: function (response) {
                T1Query.getForm().findField('P2').setValue('');

                docnoQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        docnoQueryStore.add({ VALUE: '', COMBITEM: '' });
                        for (var i = 0; i < tb_data.length; i++) {
                            docnoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                    else
                        Ext.Msg.alert('訊息', '查不到待點收申請單');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 整張申請單點收完成
    // setType = 0: 所有品項已完成點收, 只更新申請單狀態
    // setType = 1: 尚有品項未點收, 先設定未點收項目的點收資料後再更新申請單狀態
    function docComplete(docno, setType) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: docCompleteSet,
            params: {
                p0: docno,
                p1: setType
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    setDocnoData(T1Query.getForm().findField('P0').getValue());
                    T1Store.removeAll();
                    T1Query.down('#btnDocComp').setDisabled(true);
                    msglabel('整張申請單點收完成');
                }
            },
            failure: function (response, options) {
                myMask.hide();
                msglabel('整張申請單點收失敗');
            }
        });
    }

    function chkBcWhpick() {
        Ext.Ajax.request({
            url: '/api/AB0091/chkBcWhpick',
            method: reqVal_p,
            params: {
                docno: T1Query.getForm().findField('P2').getValue()
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg != '0') {
                        Ext.Msg.alert('訊息', '此申請單資料尚有' + data.msg + '項未揀貨出庫');
                    }
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    // 檢查院內碼
    function chkMmcode(mmcode) {
        Ext.Ajax.request({
            url: chkMmcodeGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: mmcode
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg; // 若有查詢到對應資料則回傳MMCODE
                    if (rtnStr == 'mmnotfound') {
                        selType = 0;
                        T1Grid.down('#COL_DOCNO').setVisible(false);
                        T1Grid.down('#COL_BASE_UNIT').setVisible(true);

                        // 重置點收倍率
                        gACKTIMES = 1;

                        Ext.Msg.alert('訊息', '查無目前待點收品項', function () { T1Query.getForm().findField('P3').setValue(''); });
                    }
                    //else if (rtnStr == 'mmdup') {
                    //    T1LastRec = T1Store.findRecord('MMCODE', mmcode);
                    //    popItemDetail("D");
                    //}
                    else if (rtnStr == 'querydoc') {
                        // 重置點收倍率
                        gACKTIMES = 1;

                        if (T1Query.getForm().findField('P0').getValue()) {
                            selType = 1;
                            T1Grid.down('#COL_DOCNO').setVisible(true);
                            T1Grid.down('#COL_BASE_UNIT').setVisible(false);
                            T1Load();
                        }
                        else
                            Ext.Msg.alert('訊息', '沒有指定部門，無法查詢');

                    }
                    else {
                        selType = 0;
                        T1Grid.down('#COL_DOCNO').setVisible(false);
                        T1Grid.down('#COL_BASE_UNIT').setVisible(true);
                        T1Load();
                        setTimeout(function () {
                            T1LastRec = T1Store.findRecord('MMCODE', rtnStr);
                            popItemDetail("U");
                        }, 300);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 於點收明細資料掃描條碼,若掃得條碼是包裝條碼,則取轉換率做累加處理
    function chkScanner(barcode) {
        Ext.Ajax.request({
            url: chkScannerGet,
            params: {
                p0: barcode
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    T1Query.getForm().findField('P2').setValue(T1LastDoc);

                    var pForm = Ext.ComponentQuery.query('panel[id=itemDetail]')[0];

                    pForm.getForm().findField('ACKQTY').setReadOnly(true);
                    pForm.getForm().findField('ADJQTY').setVisible(false);
                    pForm.down('#tempsubmit').setVisible(false);

                    if (rtnStr == 'notfound')
                        msglabel('條碼' + barcode + '不存在');
                    else {
                        var getMmcode = rtnStr.split('^')[0];
                        var getTratio = rtnStr.split('^')[1];

                        if (pForm.getForm().findField('MMCODE').getValue() == getMmcode) {
                            if (parseInt(getTratio) > 0) {
                                // 若本次掃描得到的條碼與目前明細資料相同,則將TRATIO加到ACKQTY
                                if (pForm.getForm().findField('ACKQTY').getValue() == '' || pForm.getForm().findField('ACKQTY').getValue() == null)
                                    pForm.getForm().findField('ACKQTY').setValue('0');
                                if (pForm.getForm().findField('ACKTIMES').getValue() == '' || pForm.getForm().findField('ACKTIMES').getValue() == null)
                                    pForm.getForm().findField('ACKTIMES').setValue('1');
                                // 本次異動(增加量)
                                var ackqtyAll = parseInt(getTratio) * parseInt(pForm.getForm().findField('ACKTIMES').getValue());

                                // 以異動量更新資料庫的值,再取得最新的值更新畫面(因應可能有多人同時點同品項)
                                Ext.Ajax.request({
                                    url: syncAckqty,
                                    params: {
                                        docno: T1LastRec.data['DOCNO'],
                                        mmcode: T11Store.data.items[docnoActNo].data['MMCODE'],
                                        adjqty: ackqtyAll
                                    },
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            var rtnStr = data.msg;

                                            pForm.getForm().findField('ACKQTY').setValue(rtnStr);

                                            var pick_remain = parseInt(pForm.getForm().findField('PICK_QTY').getValue()) - parseInt(pForm.getForm().findField('ACKQTY').getValue());
                                            if (pick_remain < 0)
                                                pick_remain = 0;
                                            msglabel(getMmcode + '點收+' + ackqtyAll + ',尚需' + pick_remain);

                                            // 數量加完後重置點收倍率
                                            gACKTIMES = 1;
                                            pForm.getForm().findField('ACKTIMES').setValue('1');

                                            // 若點收數量已跟待收(核撥)數量相同,儲存資料
                                            if (pForm.getForm().findField('ACKQTY').getValue() == pForm.getForm().findField('PICK_QTY').getValue()) {
                                                updateQty(pForm.getForm().findField('ACKQTY').getValue(), 'U');
                                            }
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }
                        }
                        else {
                            // 若查到資料的院內碼跟目前點收院內碼不同

                            // 儲存點收倍率供下一品項使用
                            gACKTIMES = parseInt(pForm.getForm().findField('ACKTIMES').getValue());

                            // 若點收數量已跟待收(核撥)數量相同,儲存當前點收量
                            if (pForm.getForm().findField('ACKQTY').getValue() == pForm.getForm().findField('PICK_QTY').getValue()) {
                                updateQty(pForm.getForm().findField('ACKQTY').getValue(), 'U');
                                setTimeout(function () {
                                    T1Query.getForm().findField('P3').setValue('');
                                    T1Query.getForm().findField('P3').setValue(barcode);
                                }, 300);
                            }
                            else // 若點收數量已跟待收(核撥)數量不同,暫存當前點收量
                            {
                                updateTmpAdjqty(0, getMmcode, barcode, true);
                            }
                        }
                    }

                    // 掃描處理完清除原條碼,以能應付連續掃描同條碼的情況
                    pForm.getForm().findField('SCANNER').setValue('');
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 檢查登入者所屬部門
    function chkUserWhno() {
        Ext.Ajax.request({
            url: chkUserWhnoGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        T1Query.getForm().findField('P0').setValue(tb_data[0].WH_NO);
                        // T1Query.getForm().findField('P1').setValue(tb_data[0].WH_NAME);
                        setDocnoData(tb_data[0].WH_NO);
                    }
                    else
                        Ext.Msg.alert('訊息', '查不到您所屬部門資料');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    chkUserWhno();

    Ext.getDoc().dom.title = T1Name;
    T1Query.getForm().findField('P3').focus(); // 預設focus在院內碼,方便其他條件皆可自動代入時,可直接掃條碼
});
