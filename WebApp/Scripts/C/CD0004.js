Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "點選申請單編號"; // 2020/2/27 藥庫與中央庫分開 藥庫:CD0004, 中央庫:CD0011
    var T2Get = '/api/CD0004/ValidAll';
    var T3Get = '/api/CD0004/PickAll';
    var actPickQtySet = '/api/CD0004/SetActPickQty';
    var pickAllSet = '/api/CD0004/SetPickAll';
    var shipoutAllSet = '/api/CD0004/SetShipoutAll';
    var chkBarcodeGet = '/api/CD0004/ChkBarcode';
    var chkMmcodeGet = '/api/CD0004/ChkMmcode';
    var chkPickCntGet = '/api/CD0004/chkPickCnt';
    var chkUserWhnoGet = '/api/CD0004/GetChkUserWhno';
    var pickUserComboGet = '/api/CD0004/GetPickUserCombo';
    var lotNoComboGet = '/api/CD0004/GetLotNoCombo';
    var docnoComboGet = '/api/CD0004/GetDocnoCombo';
    var lotNoDataComboGet = '/api/CD0004/GetLotNoDataCombo';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1LastMmcode = '';
    var T2LastRec = null;
    var T2LastRecIdx = 0;
    var showDetail = false;
    var rtnType = 0; // 0: 揀貨確認; 1: 揀貨取消(刪除)
    var isClick = false; // 是否透過Grid點選品項
    var docnoScan = ''; // 透過掃描方式取得的申請單

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['LOT_NO_F', 'VALID_DATE', 'ACT_PICK_QTY']
    });
    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'PICK_DATE', 'DOCNO', 'SEQ', 'MMNAME_E', 'BARCODE', 'MMCODE']
    });

    // 揀貨人員
    var pickUserQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 揀貨批次
    var lotNoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 揀貨申請單
    var docnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 揀貨批次明細
    var lotNoDataStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'MMCODE', 'LOT_NO', 'VALID_DATE', 'INV_QTY']
    });

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P0',
        id: 'P0',
        margin: '1 0 1 0',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        allowBlank: false,
        storeAutoLoad: true,
        limit: 20,
        queryUrl: '/api/CD0004/GetWH_NoCombo',
        extraFields: ['WH_NO', 'WH_NAME'],
        getDefaultParams: function () {
        },
        labelStyle: 'width: 40%',
        listeners: {
            select: function (c, r, i, e) {
                var f = T1Query.getForm();
                if (r.get('WH_NAME') !== '') {
                    f.findField("P1").setValue(r.get('WH_NAME'));
                    setPickUserData(r.get('WH_NO'), false);
                    setLotNoData(r.get('WH_NO'));
                    setDocnoData(r.get('WH_NO'));

                    T1Grid.down('#COL_MMNAME_C').setVisible(false);
                    T1Grid.down('#COL_MMNAME_E').setVisible(true);
                    T1Query.down('#btnShipoutAll').setVisible(false);
                    T1Query.down('#btnPickAll').setVisible(false);
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
    });

    // 供相機掃描取得的條碼
    Ext.define('CameraSharedData', {
        callSubmitBack: function () {
            T1Query.getForm().findField('P5').setValue(CameraSharedData.selItem);
        },
        selItem: '',
        selQty: '',
        singleton: true    //no singleton, no return
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
            labelStyle: 'width: 45%',
            width: '30%'
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
                        text: '藥品進貨接收',
                        handler: function () {
                            parent.link2('/Form/Index/CC0005', ' 藥品進貨接收(PDA)', true);
                            //parent.link2('/Form/Index/CE0016','初盤數量輸入作業');
                        },
                        width: '42%',
                    },
                ]
            },
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: {
                    type: 'box',
                    vertical: true,
                    align: 'stretch'
                },
                items: [

                    wh_NoCombo, {
                        xtype: 'displayfield',
                        fieldLabel: '庫房名稱',
                        name: 'P1',
                        id: 'P1',
                        margin: '1 0 1 0',
                        enforceMaxLength: true,
                        labelStyle: 'width: 40%'
                    }
                ]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: {
                    type: 'box',
                    vertical: true,
                    align: 'stretch'
                },
                items: [
                    {
                        xtype: 'combo',
                        store: pickUserQueryStore,
                        displayField: 'COMBITEM',
                        valueField: 'VALUE',
                        id: 'P3',
                        name: 'P3',
                        margin: '1 0 1 0',
                        fieldLabel: '揀貨人員',
                        margin: '1 0 1 0',
                        fieldCls: 'required',
                        allowBlank: false,
                        queryMode: 'local',
                        autoSelect: true,
                        editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                        labelStyle: 'width: 40%',
                        listeners: {
                            focus: function (field, event, eOpts) {
                                if (!field.isExpanded) {
                                    setTimeout(function () {
                                        field.expand();
                                    }, 300);
                                }
                            },
                            change: function (field, nValue, oValue, eOpts) {
                                var f = T1Query.getForm();
                                setLotNoData(f.findField('P0').getValue());
                                setDocnoData(f.findField('P0').getValue());
                                // f.findField('P4').setDisabled(false);
                            }
                        }
                    }, {
                        xtype: 'checkbox',
                        id: 'P7',
                        name: 'P7',
                        margin: '4 0 4 4',
                        boxLabel: '顯示全部查詢結果',
                        value: 'P7',
                        checked: false
                    }, {
                        xtype: 'panel',
                        border: false,
                        margin: '1 0 1 0',
                        width: '30%',
                        layout: {
                            type: 'hbox',
                            vertical: false
                        },
                        items: [
                            {
                                xtype: 'radiofield',
                                id: 'P4_SEL',
                                name: 'P4_SEL',
                                width: '10%',
                                padding: '4% 0 0 4%',
                                // checked: true,
                                listeners: {
                                    change: function (field, nVal, oVal, eOpts) {
                                        if (nVal) {
                                            enableField('P4');
                                            T1Query.getForm().findField('P2').setValue(new Date());
                                        }
                                    }
                                }
                            }, {
                                xtype: 'datefield',
                                id: 'P2',
                                name: 'P2',
                                margin: '1 0 1 0',
                                fieldLabel: '揀貨日期',
                                disabled: true,
                                width: '90%',
                                labelStyle: 'width: 45%',
                                //fieldCls: 'required',
                                allowBlank: false,
                                margin: '1 0 1 0',
                                labelStyle: 'width: 40%',
                                listeners: {
                                    change: function (fiels, nValue, oValue, eOpts) {
                                        var f = T1Query.getForm();
                                        //setPickUserData(f.findField('P0').getValue(), false);
                                        //setLotNoData(f.findField('P0').getValue());
                                    },
                                    focus: function (field, event, eOpts) {
                                        docnoScan = '';
                                        if (!field.isExpanded) {
                                            setTimeout(function () {
                                                field.expand();
                                            }, 300);
                                        }
                                    }
                                }
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        border: false,
                        margin: '1 0 1 0',
                        width: '30%',
                        layout: {
                            type: 'hbox',
                            vertical: false
                        },
                        items: [
                            {
                                xtype: 'combo',
                                store: lotNoQueryStore,
                                displayField: 'COMBITEM',
                                valueField: 'VALUE',
                                id: 'P4',
                                name: 'P4',
                                margin: '1 0 1 0',
                                disabled: true,
                                fieldLabel: '依分配批次',
                                margin: '1 0 1 0',
                                queryMode: 'local',
                                autoSelect: true,
                                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                                width: '90%',
                                labelStyle: 'width: 55%',
                                listeners: {
                                    focus: function (field, event, eOpts) {
                                        if (!field.isExpanded) {
                                            setTimeout(function () {
                                                field.expand();
                                            }, 300);
                                        }
                                    }
                                }
                            }
                        ]
                    }, {
                        xtype: 'panel',
                        border: false,
                        margin: '1 0 1 0',
                        width: '30%',
                        layout: {
                            type: 'hbox',
                            vertical: false
                        },
                        items: [
                            {
                                xtype: 'radiofield',
                                id: 'P6_SEL',
                                name: 'P6_SEL',
                                width: '10%',
                                padding: '4% 0 0 4%',
                                checked: true,
                                listeners: {
                                    change: function (field, nVal, oVal, eOpts) {
                                        if (nVal) {
                                            enableField('P6');
                                        }
                                    }
                                }
                            },
                            {
                                xtype: 'textfield',
                                    id: 'P6',
                                name: 'P6',
                                fieldLabel: '依申請單號',
                                width: '90%',
                                labelStyle: 'width: 45%',
                                listeners: {
                                    change: function (field, nValue, oValue, eOpts) {
                                        if (nValue.length >= 12) { 
                                           getDocnoInfo(nValue);
                                        }
                                    }
                                }
                            }
                        ]
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '申請單位',
                        name: 'P6_1',
                        id: 'P6_1',
                        width: '90%',
                        labelWidth: false,
                        labelStyle: 'width: 40%'
                    }
                ]
            }, {
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
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
                        width: '30%',
                        items: [
                            {
                                xtype: 'textfield',
                                fieldLabel: '條碼',
                                name: 'P5',
                                id: 'P5',
                                enforceMaxLength: true,
                                maxLength: 100,
                                width: '90%',
                                labelWidth: false,
                                labelStyle: 'width: 29%',
                                listeners: {
                                    change: function (field, nValue, oValue, eOpts) {
                                        if (nValue.length >= 8) { // 2020.06.15依林江提供,藥庫院內碼只會有8碼
                                            chkBarcode(nValue);
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
                        xtype: 'panel',
                        border: false,
                        margin: '1 0 1 0',
                        layout: {
                            type: 'hbox',
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
                                if (!f.isValid())
                                    Ext.Msg.alert('提醒', '查詢條件為必填');
                                else {
                                    var p4 = T1Query.getForm().findField('P4').getValue();
                                    var p6 = T1Query.getForm().findField('P6').getValue();
                                    if ((p4 == '' || p4 == null) && (p6 == '' || p6 == null)) {
                                        Ext.Msg.alert('提醒', '分配批次或申請單號至少需填寫一個');
                                    }
                                    else {
                                        showDetail = true;
                                        // Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                        setQueryFieldVisible(false);
                                        T1Load();
                                        msglabel('');
                                    }
                                }
                            }
                        }, {
                            xtype: 'button',
                            text: '清除',
                            width: '42%',
                            margin: '1',
                            handler: function () {
                                var f = this.up('form').getForm();
                                //f.reset();
                                f.findField('P4').setValue('');
                                f.findField('P6').setValue('');
                                f.findField('P6_1').setValue('');
                                f.findField('P5').setValue('');
                                f.findField('P5').focus();
                                msglabel('');
                            }
                        }, {
                            xtype: 'button',
                            text: '<font size="2vmin">●</font>',
                            itemId: 'visibleBtn',
                            width: '7%',
                            margin: '0 0 0 8',
                            handler: function () {
                                var f = T1Query.getForm();
                                if (f.findField('P0').hidden) {
                                    setQueryFieldVisible(true);
                                }
                                else {
                                    setQueryFieldVisible(false);
                                }
                            }
                        }]
                    }, , {
                        xtype: 'panel',
                        border: false,
                        margin: '1 0 1 0',
                        layout: {
                            type: 'hbox',
                        },
                        items: [{
                            xtype: 'button',
                            text: '整批出庫',
                            itemId: 'btnShipoutAll',
                            disabled: true,
                            hidden: true,
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    width: '100%',

                                },
                                'width >= 600': {
                                    hidden: false
                                }
                            },
                            margin: '1',
                            handler: function () {
                                Ext.MessageBox.confirm('提醒', '確定將已揀貨品項全部設定出庫?', function (btn, text) {
                                    if (btn === 'yes') {
                                        shipoutAll();
                                    }
                                });
                            }
                        }, {
                            xtype: 'button',
                            text: '整批揀貨',
                            itemId: 'btnPickAll',
                            disabled: true,
                            plugins: 'responsive',
                            responsiveConfig: {
                                'width < 600': {
                                    width: '50%',
                                    hidden: true
                                },
                                'width >= 600': {
                                    hidden: false
                                }
                            },
                            margin: '1',
                            handler: function () {
                                Ext.MessageBox.confirm('提醒', '確定將查詢的待揀貨非批號管制品項均設定已揀貨?', function (btn, text) {
                                    if (btn === 'yes') {
                                        updatePickAll();
                                    }
                                });
                            }
                        }]
                    }
                ]
            }]
    });

    var T2Query = Ext.widget({
        itemId: 'form2',
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        width: '100%',
        collapsible: false,
        hideCollapseTool: true,
        titleCollapse: false,
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: false,
            labelStyle: 'width: 45%',
            width: '30%'
        },
        items: [{
            xtype: 'button',
            text: '返回',
            margin: '1',
            handler: function () {
                var layout = viewport.down('#t1Main').getLayout();
                layout.setActiveItem(0);
                if (T1LastRec.data['ACT_PICK_QTY'] > 0)
                    popItemDetail('D');
                else
                    popItemDetail('U');
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BcWhpick', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue(),
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (!successful) {
                    alert('查詢失敗');
                    T1Store.removeAll();
                    T1Tool.onLoad();
                }
                else if (records.length > 0) {
                    var showNextRec = false;
                    if (records[0].data['MAT_CLASS'] == '01') {
                        // MAT_CLASS=01(藥庫申請單),直接跳出填寫視窗
                        T1LastRec = records[0];
                        if (T1LastRec && showDetail) {
                            T1Rec = 1;
                            if (T1LastRec.data['ACT_PICK_QTY'])
                                popItemDetail('D');
                            else
                                popItemDetail('U');
                            showDetail = false;
                        }
                        else
                            showNextRec = true;
                    }

                    // 顯示下一筆尚未揀貨品項
                    if (showNextRec) {
                        var getFirstRec = false;
                        var chkIdx = 0; // 本次填寫的資料索引
                        if (T1LastMmcode && rtnType == 0) // 已填寫過一筆揀貨且不是做揀貨取消
                        {
                            chkIdx = store.findExact('MMCODE', T1LastMmcode);
                            if (chkIdx < 0)
                                chkIdx = 0;
                            for (var i = chkIdx; i < records.length; i++) {
                                if (!(records[i].data['ACT_PICK_QTY'])) {
                                    T1LastRec = records[i];
                                    getFirstRec = true;
                                    break;
                                }
                            }
                            // 填完一筆後跳到下一筆尚未填寫揀貨量的記錄
                            if (getFirstRec)
                                popItemDetail('U');
                        }
                    }

                    T1Query.down('#btnPickAll').setDisabled(false);
                }
                else if (records.length == 0) {
                    T1Query.down('#btnPickAll').setDisabled(true);
                    Ext.Msg.alert('訊息', '查無對應揀貨中項目', function () { T1Query.getForm().findField('P5').setValue(''); });
                }
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
                    p0: T1LastRec.data['WH_NO'],
                    p1: T1LastRec.data['PICK_DATE'],
                    p2: T1LastRec.data['DOCNO'],
                    p3: T1LastRec.data['SEQ']
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
    var T3Store = Ext.create('Ext.data.Store', {
        model: 'T3Model',
        pageSize: 200,
        remoteSort: true,
        sorters: [{ property: 'STORE_LOC', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['WH_NO'],
                    p1: T1LastRec.data['PICK_DATE'],
                    p2: T1LastRec.data['DOCNO'],
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
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
            url: T3Get,
            reader: {
                type: 'json',
                root: 'etts',
                totalProperty: 'rc'
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }
    function T2Load() {
        T2Store.load({
            params: {
                start: 0
            }
        });
    }
    function T3Load() {
        T3Store.load({
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
                width: '40%'
            }, {
                text: "申請單位",
                dataIndex: 'APPDEPT',
                width: '30%'
            }, {
                text: "申請備註",
                dataIndex: 'APPLY_NOTE',
                width: '30%'
            }, {
                text: "備註",
                dataIndex: 'APLYITEM_NOTE',
                width: '30%'
            }, {
                text: "項次",
                dataIndex: 'SEQ',
                width: '20%'
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '20%'
            }, {
                text: "中文品名",
                itemId: 'COL_MMNAME_C',
                dataIndex: 'MMNAME_C',
                width: '40%',
                plugins: '',
                renderer: function (val, meta, record) {
                    if (record.data['ACT_PICK_USERID']) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else
                        return val;
                }
            }, {
                text: "英文品名",
                itemId: 'COL_MMNAME_E',
                dataIndex: 'MMNAME_E',
                width: '40%',
                plugins: ''
            }, {
                text: "核撥量",
                dataIndex: 'APPQTY',
                width: '20%',
                plugins: '',
                renderer: function (val, meta, record) {
                    if (record.data['ACT_PICK_USERID']) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else
                        return val;
                }
            }, {
                text: "撥發量",
                dataIndex: 'ACT_PICK_QTY',
                width: '20%'
            }, {
                text: "撥補單位",
                dataIndex: 'BASE_UNIT',
                width: '20%'
            }, {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                width: '30%',
                plugins: '',
                renderer: function (val, meta, record) {
                    if (record.data['ACT_PICK_USERID']) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else
                        return val;
                }
            }, {
                text: "批號效期註記",
                dataIndex: 'WEXP_ID',
                width: '15%'
            }
                //, {
                //    text: " ",
                //    align: 'left',
                //    width: '10%',
                //    plugins: 'responsive',
                //    responsiveConfig: {
                //        'width < 600': {
                //            flex: 1
                //        },
                //        'width >= 600': {
                //            flex: 0
                //        }
                //    },
                //    renderer: function (val, meta, record) {
                //        if (record.data['ACT_PICK_QTY'] > 0)
                //        {
                //            // return '<a href=\'javascript:popItemDetail("D");\'>取消</a>';
                //            return '<a href=\'javascript:popItemDetail("D");\'>明細</a>';
                //        }
                //        else
                //            return '<a href=\'javascript:popItemDetail("U");\'>揀貨</a>';
                //    }
                //}
                , {
                text: " ",
                align: 'left',
                flex: 1
            }]
        },
        viewConfig: {
            getRowClass: function (record, rowIndex, rowParams, store) {
                if (T1Query.getForm().findField('P0').getValue() == '560000')
                    return 'multiline-row';
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            },
            itemclick: function (view, record, item, index, e, eOpts) {
                if (T1LastRec != null) {
                    if (T1Query.getForm().findField('P0').getValue() == '560000') {
                        if (parseInt(T1LastRec.data['SHIPOUTCNT']) > 0)
                            popItemDetail('D'); // 已有出庫資料則不可修改
                        else
                            popItemDetail('U');
                    }
                    else {
                        isClick = true;
                        if (T1LastRec.data['ACT_PICK_QTY'] > 0)
                            popItemDetail('D');
                        else
                            popItemDetail('U');
                    }
                }
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        plugins: 'bufferedrenderer',
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Query]
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
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: '20%'
            }, {
                text: "揀貨日期",
                dataIndex: 'PICK_DATE',
                width: '20%'
            }, {
                text: "申請單號碼",
                dataIndex: 'DOCNO',
                width: '30%'
            }, {
                text: "項次",
                dataIndex: 'SEQ',
                width: '10%'
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '40%',
                plugins: ''
            }, {
                text: "條碼",
                dataIndex: 'BARCODE',
                width: '30%',
                plugins: ''
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: '30%',
                plugins: ''
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
            }
        }
    });

    var popform = Ext.create('Ext.form.Panel', {
        id: 'itemDetail',
        height: '100%',
        //layout: 'fit',
        closable: false,
        scrollable: false,
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
                id: 'pickDetail',
                layout: 'vbox',
                padding: '2vmin',
                scrollable: false,
                items: [{
                    xtype: 'displayfield',
                    fieldLabel: '申請單號',
                    name: 'DOCNO',
                    readOnly: true,
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            labelStyle: 'width: 40%',
                        },
                        'width >= 600': {
                            labelStyle: 'width: 35%',
                        }
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '條碼',
                    name: 'BARCODE',
                    labelStyle: 'width: 40%',
                    listeners: {
                        change: function (field, nValue, oValue, eOpts) {
                            if (nValue.length >= 8) {
                                Ext.Ajax.request({
                                    url: chkMmcodeGet,
                                    params: {
                                        p0: popform.getForm().findField('BARCODE').getValue()
                                    },
                                    method: reqVal_p,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            var rtnMmcode = data.msg;
                                            if (rtnMmcode == popform.getForm().findField('MMCODE').getValue()) {
                                                popform.getForm().findField('BARCODE').setValue('');
                                                popform.getForm().findField('INV_QTY').focus();
                                                popform.down('#itemDetailForm').scrollTo(0, 0);
                                                msglabel('掃描品項與目前品項相符');
                                            }
                                            else {
                                                //Ext.Msg.alert('訊息', '掃描的條碼與目前品項不符!', function () { popform.getForm().findField('BARCODE').setValue(''); });
                                                // 若掃描品項與目前品項不符,改為切換到掃描的品項
                                                msglabel('');
                                                var parBarcode = popform.getForm().findField('BARCODE').getValue();
                                                T1Query.getForm().findField('P5').setValue(parBarcode);
                                                chkBarcode(parBarcode);
                                            }
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }
                        }
                    }
                }]
            }, {
                xtype: 'container',
                itemId: 'itemDetailForm',
                layout: 'vbox',
                height: '70%',
                padding: '2vmin',
                border: true,
                style: 'border: 1px solid gray',
                scrollable: true,
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '儲位',
                        name: 'STORE_LOC',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '核撥量',
                        name: 'APPQTY',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '撥發量',
                        name: 'ACT_PICK_QTY',
                        readOnly: false,
                        maskRe: /[0-9.]/,
                        regexText: '只能輸入數字',
                        regex: /^([1-9]\d*(\.\d*)?|0)$/,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        },
                        selectOnFocus: true,
                        enableKeyEvents: true,
                        listeners: {
                            keydown: function (field, e, eOpts) {
                                if (e.keyCode == '38') // 按↑
                                    popform.getForm().findField('ACT_PICK_NOTE').focus();
                                else if (e.keyCode == '40') // 按↓
                                    popform.getForm().findField('INV_QTY').focus();
                                else if (e.keyCode == '13') // 按enter
                                    popform.down('#confirmsubmit').click();
                            }
                        }
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '總庫存量',
                        name: 'INV_QTY',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        },
                        enableKeyEvents: true,
                        listeners: {
                            keydown: function (field, e, eOpts) {
                                //if (e.keyCode == '38') // 按↑
                                //    popform.getForm().findField('ACT_PICK_QTY').focus();
                                //else if (e.keyCode == '40') // 按↓
                                //    popform.getForm().findField('ACT_PICK_NOTE').focus();
                                if (e.keyCode == '13') // 按enter則focus到撥發量
                                    popform.getForm().findField('ACT_PICK_QTY').focus();
                            }
                        }
                    }, {
                        xtype: 'container',
                        padding: '2vmin',
                        itemId: 'validGrid',
                        width: '100%',
                        items: [
                            {
                                xtype: 'container',
                                items: [
                                    {
                                        xtype: 'button',
                                        text: '新增',
                                        itemId: 'btnAdd',
                                        labelAlign: 'center',
                                        handler: function () {
                                            T2Store.add({
                                                VALID_DATE: '', ACT_PICK_QTY: ''
                                            });
                                        }
                                    }, {
                                        xtype: 'button',
                                        text: '刪除',
                                        itemId: 'btnDel',
                                        labelAlign: 'center',
                                        disabled: true,
                                        handler: function () {
                                            Ext.MessageBox.confirm('訊息', '是否確定刪除此效期數量資料</br>(' + T2LastRec.data.LOT_NO + ')?', function (btn, text) {
                                                if (btn === 'yes') {
                                                    T2Store.remove(T2LastRec);
                                                    popform.down('#btnDel').setDisabled(true);
                                                }
                                            });
                                        }
                                    }, {
                                        xtype: 'label',
                                        html: '<b><font size="4vw" color="blue"> 批號效期 </font></b> '
                                    }
                                ]
                            }, {
                                xtype: 'grid',
                                store: T2Store,
                                itemId: 'T2Grid',
                                sortableColumns: false,
                                colspan: 2,
                                width: '90%',
                                height: '300',
                                scrollable: true,
                                selType: "cellmodel",
                                plugins: [
                                    Ext.create("Ext.grid.plugin.CellEditing", {
                                        clicksToEdit: 1//控制點擊幾下啟動編輯
                                    })
                                ],
                                columns: [
                                    {
                                        text: "批號",
                                        dataIndex: 'LOT_NO',
                                        align: 'right',
                                        style: 'text-align:center',
                                        width: '30%',
                                        editor: {
                                            xtype: 'combo',
                                            store: lotNoDataStore,
                                            displayField: 'LOT_NO',
                                            valueField: 'LOT_NO',
                                            queryMode: 'local',
                                            autoSelect: true,
                                            editable: true, typeAhead: true, selectOnFocus: true,
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            field.expand();
                                                        }, 300);
                                                    }
                                                },
                                                change: function (field, nValue, oValue, eOpts) {

                                                },
                                                blur: function (field, eOpts) {
                                                    var actPickQtySum = 0;
                                                    for (var i = 0; i < T2Store.data.length; i++) {
                                                        if (field.rawValue == T2Store.data.items[i].data['LOT_NO']) {
                                                            Ext.Msg.alert('訊息', '已選擇此批號!');
                                                            field.setValue('');
                                                            T2Store.data.items[T2LastRecIdx].data['VALID_DATE'] = '';
                                                            T2Store.data.items[T2LastRecIdx].data['ACT_PICK_QTY'] = '';
                                                            T2Store.loadRecords(T2Store.getRange());
                                                            return false;
                                                        }

                                                        // 統計目前已輸入批號數量
                                                        var tmpQty = 0;
                                                        if (T2Store.data.items[i].data['ACT_PICK_QTY'] != '')
                                                            tmpQty = parseInt(T2Store.data.items[i].data['ACT_PICK_QTY']);
                                                        actPickQtySum += tmpQty;
                                                    }

                                                    var f_lot_data = lotNoDataStore.findRecord('LOT_NO', field.rawValue);
                                                    var newQty = parseInt(popform.getForm().findField('ACT_PICK_QTY').getValue()) - actPickQtySum;
                                                    if (newQty < 0)
                                                        newQty = 0;

                                                    setTimeout(function () {
                                                        if (f_lot_data) {
                                                            T2Store.data.items[T2LastRecIdx].data['VALID_DATE'] = f_lot_data.data['EXP_DATE'];
                                                            T2Store.data.items[T2LastRecIdx].data['ACT_PICK_QTY'] = newQty;
                                                            T2Store.loadRecords(T2Store.getRange());
                                                        }
                                                    }, 100);
                                                }
                                            }
                                        }
                                    }, {
                                        text: "有效日期",
                                        dataIndex: 'VALID_DATE',
                                        align: 'center',
                                        style: 'text-align:center',
                                        width: '35%',
                                        editor: {
                                            xtype: "datefield",
                                            enforceMaxLength: true, //最多輸入的長度有限制
                                            maxLength: 7,
                                            minValue: new Date,
                                            maskRe: /[0-9]/,
                                            listeners: {
                                                focus: function (field, event, eOpts) {
                                                    if (!field.isExpanded) {
                                                        setTimeout(function () {
                                                            field.expand();
                                                        }, 300);
                                                    }
                                                },
                                                blur: function (field, eOpts) {
                                                    field.rawDate = field.rawValue;
                                                }
                                            }
                                        }
                                    }, {
                                        text: "撥發數量",
                                        dataIndex: 'ACT_PICK_QTY',
                                        align: 'right',
                                        style: 'text-align:center',
                                        width: '30%',
                                        editor: {
                                            xtype: 'textfield',
                                            selectOnFocus: true,
                                            maskRe: /[0-9.]/,
                                            regexText: '只能輸入數字',
                                            regex: /^[1-9]\d*(.\d*)?$/
                                        }
                                    }
                                ],
                                listeners: {
                                    select: function (model, records) {
                                        T2LastRecIdx = model.nextSelection.rowIdx;
                                        T2LastRec = records;
                                        popform.down('#btnDel').setDisabled(false);
                                    }
                                }
                            }
                        ]
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '申請單位', //抓資料欄位原為申請庫別(APPDEPT), 改為申請單位(TOWH)
                        name: 'APPDEPT',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%',
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%',
                            }
                        }
                    }, {
                        xtype: 'textarea',
                        fieldLabel: '揀貨備註',
                        name: 'ACT_PICK_NOTE',
                        readOnly: false,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%'
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%'
                            }
                        },
                        enableKeyEvents: true,
                        listeners: {
                            keydown: function (field, e, eOpts) {
                                if (e.keyCode == '38') // 按↑
                                    popform.getForm().findField('INV_QTY').focus();
                                else if (e.keyCode == '40') // 按↓
                                    popform.getForm().findField('ACT_PICK_QTY').focus();
                                else if (e.keyCode == '13') // 按enter
                                    popform.down('#confirmsubmit').click();
                            }
                        }
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '申請備註',
                        name: 'APLYITEM_NOTE',
                        readOnly: true,
                        plugins: 'responsive',
                        responsiveConfig: {
                            'width < 600': {
                                labelStyle: 'width: 40%'
                            },
                            'width >= 600': {
                                labelStyle: 'width: 35%'
                            }
                        }
                    }
                ]
            }, {
                xtype: 'container',
                layout: 'vbox',
                padding: '2vmin',
                height: '15%',
                scrollable: true,
                items: [{
                    xtype: 'panel',
                    layout: 'hbox',
                    border: false,
                    width: '100%',
                    items: [{
                        xtype: 'displayfield',
                        fieldLabel: '應撥',
                        name: 'NEED_PICK_ITEMS',
                        readOnly: true,
                        width: '35%',
                        labelStyle: 'width: 60%'
                    }, {
                        xtype: 'displayfield',
                        fieldLabel: '未撥',
                        name: 'LACK_PICK_ITEMS',
                        readOnly: true,
                        width: '35%',
                        labelStyle: 'width: 60%'
                    }, {
                        xtype: 'button',
                        text: '未撥品項',
                        handler: function () {
                            var layout = viewport.down('#t1Main').getLayout();
                            layout.setActiveItem(1);
                            T3Load();
                        }
                    }]
                }]
            }
        ],
        buttons: [{
            itemId: 'confirmsubmit',
            disabled: false,
            text: '<font size="3vmin">確定</font>',
            height: '6vmin',
            handler: function () {
                if (popform.getForm().isValid()) {
                    setTimeout(function () {
                        var appQty = popform.getForm().findField('APPQTY').getValue();
                        var actPickQty = popform.getForm().findField('ACT_PICK_QTY').getValue();
                        var actPickNote = popform.getForm().findField('ACT_PICK_NOTE').getValue();
                        //var lotNo = popform.getForm().findField('LOT_NO_F').getValue();
                        //var validDate = popform.getForm().findField('VALID_DATE').getValue();
                        var totalActPickQty = 0;
                        // 若屬於效期管制項目,則統計各效期數量合計是否等於揀貨數量
                        if (T1LastRec.data['WEXP_ID'] == 'Y') {
                            for (var i = 0; i < T2Store.data.length; i++) {
                                // 若任一項目還有欄位未填寫
                                if (!T2Store.data.items[i].data['LOT_NO'] || !T2Store.data.items[i].data['VALID_DATE'] || !T2Store.data.items[i].data['ACT_PICK_QTY']) {
                                    Ext.Msg.alert('訊息', '尚有效期項目未填入批號,有效日期或撥發數量!');
                                    return false;
                                }
                                // 逐項累加各效期項目揀貨數量
                                totalActPickQty += parseFloat(T2Store.data.items[i].data['ACT_PICK_QTY']);
                            }
                            if (totalActPickQty == 0 && actPickQty > 0) {
                                Ext.Msg.alert('訊息', '尚未設定批號效期資料');
                                return false;
                            }
                            else {
                                if (totalActPickQty != actPickQty) {
                                    Ext.Msg.alert('訊息', '各效期數量合計與撥發量不同');
                                    return false;
                                }
                            }
                        }

                        if (actPickQty != "" && actPickQty != null) {
                            //if (T1LastRec.data['WEXP_ID'] == 'Y') {
                            //    if (lotNo == '' || lotNo == null || validDate == '' || validDate == null) {
                            //        Ext.Msg.alert('訊息', '批號及效期為必填');
                            //        return false;
                            //    }
                            //}

                            rtnType = 0;
                            if (actPickQty > 0 && actPickQty != appQty) {
                                Ext.MessageBox.confirm('提醒', '您輸入的撥發量與核撥量不同</br>，是否確定要儲存資料?', function (btn, text) {
                                    if (btn === 'yes') {
                                        updateQty(actPickQty, actPickNote, 'U');
                                    }
                                });
                            }
                            else
                                updateQty(actPickQty, actPickNote, 'U');
                        }
                        else
                            Ext.Msg.alert('訊息', '撥發量為必填');

                    }, 100); // 給予一點延遲,避免操作上在填寫一筆資料的撥發量後,還沒blur就直接按確定,造成store還沒更新就開始統計ACT_PICK_QTY
                }
                else
                    Ext.Msg.alert('訊息', '輸入格式不正確');
            }
        }, {
            id: 'winclosed',
            disabled: false,
            text: '<font size="3vmin">取消</font>',
            height: '6vmin',
            handler: function () {
                rtnType = 0;

                T1Query.getForm().findField('P5').setValue('');
                T1Query.getForm().findField('P5').focus();
                msglabel('');

                var layout = viewport.down('#t1Main').getLayout();
                layout.setActiveItem(0);
            }
        }]
    });

    popItemDetail = function (setType) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        var layout = viewport.down('#t1Main').getLayout();
        layout.setActiveItem(2);

        setLotNoDataDetail(T1LastRec.data['WH_NO'], T1LastRec.data['MMCODE']);
        popform.loadRecord(T1LastRec);
        // 若撥發量尚未填寫則預設代核撥量
        if (!(popform.getForm().findField('ACT_PICK_QTY').getValue()))
            popform.getForm().findField('ACT_PICK_QTY').setValue(popform.getForm().findField('APPQTY').getValue());

        T2Store.removeAll();
        if (T1LastRec.data['WEXP_ID'] == 'Y') {
            T2Load();
            popform.down('#validGrid').setVisible(true);
            popform.getForm().findField('MMCODE').setFieldStyle('color:red');
        }
        else {
            popform.down('#validGrid').setVisible(false);
            popform.getForm().findField('MMCODE').setFieldStyle('color:black');
        }

        //if (T1LastRec.data['WEXP_ID'] == 'Y')
        //{
        //    popform.getForm().findField('LOT_NO_F').setVisible(true);
        //    popform.getForm().findField('VALID_DATE').setVisible(true);
        //}
        //else
        //{
        //    popform.getForm().findField('LOT_NO_F').setVisible(false);
        //    popform.getForm().findField('VALID_DATE').setVisible(false);
        //}

        // 檢視明細模式
        if (setType == 'D') {
            popform.down('#confirmsubmit').setDisabled(true);
            popform.getForm().findField('ACT_PICK_QTY').setReadOnly(true);
            popform.getForm().findField('ACT_PICK_NOTE').setReadOnly(true);
            //popform.getForm().findField('LOT_NO_F').setReadOnly(true);
            //popform.getForm().findField('VALID_DATE').setReadOnly(true);
            if (T1LastRec.data['WEXP_ID'] == 'Y') {
                popform.down('#btnAdd').setDisabled(true);
                popform.down('#btnDel').setDisabled(true);
            }
        }
        else // 編輯模式
        {
            popform.down('#confirmsubmit').setDisabled(false);
            popform.getForm().findField('ACT_PICK_QTY').setReadOnly(false);
            popform.getForm().findField('ACT_PICK_NOTE').setReadOnly(false);
            //popform.getForm().findField('LOT_NO_F').setReadOnly(false);
            //popform.getForm().findField('VALID_DATE').setReadOnly(false);
            if (T1LastRec.data['WEXP_ID'] == 'Y') {
                popform.down('#btnAdd').setDisabled(false);
                popform.down('#btnDel').setDisabled(true);
            }
        }

        popform.getForm().findField('MMNAME_C').setVisible(false);
        popform.getForm().findField('MMNAME_E').setVisible(true);

        setTimeout(function () {
            if (isClick) {
                popform.getForm().findField('BARCODE').focus(); // 透過點選清單方式進入品項,則需先掃描條碼
                msglabel('請先掃描一次條碼');
            }
            else {
                //popform.getForm().findField('INV_QTY').focus();
                // 現在不論使用點清單進入或掃描進入,都focus在條碼欄
                popform.getForm().findField('BARCODE').focus(); // 透過點選清單方式進入品項,則需先掃描條碼
                msglabel('請先掃描一次條碼');
            }
            isClick = false;
            popform.down('#itemDetailForm').scrollTo(0, 0);

            myMask.hide();
        }, 500);
    }

    // 更新或刪除撥發量
    function updateQty(actPickQty, actPickNote, setType) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        var records = T2Store.getRange();
        var submitS = ''; // 單筆資料
        var submitT = ''; // 全部資料
        if (records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                submitS = records[i].data.LOT_NO + "^" + records[i].data.VALID_DATE + "^" + records[i].data.ACT_PICK_QTY;
                if (i == records.length - 1)
                    submitT += submitS;
                else
                    submitT += submitS + "ˋ";
            }
        }
        var pickUserid = T1LastRec.data['PICK_USERID'];
        if (pickUserid == '' || pickUserid == null)
            pickUserid = T1Query.getForm().findField('P3').getValue();

        Ext.Ajax.request({
            url: actPickQtySet,
            params: {
                wh_no: T1LastRec.data['WH_NO'],
                pick_date: T1LastRec.data['PICK_DATE'],
                docno: T1LastRec.data['DOCNO'],
                seq: T1LastRec.data['SEQ'],
                pick_userid: pickUserid,
                lot_no: T1LastRec.data['LOT_NO'],
                mmcode: T1LastRec.data['MMCODE'],
                wexp_id: T1LastRec.data['WEXP_ID'],
                act_pick_qty: actPickQty,
                act_pick_note: actPickNote,
                //lot_no_f: setLotNo,
                //valid_date: setValidDate,
                setType: setType,
                submitt: submitT
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);

                if (data.sucess = false) {
                    Ext.Msg.alert('錯誤', data.msg);
                    return;
                }

                T1LastMmcode = T1LastRec.data['MMCODE'];
                //setPickUserData(T1Query.getForm().findField('P0').getValue(), false);
                // 資料更新後重載並關閉視窗
                T1Query.getForm().findField('P5').setValue('');
                T1Query.getForm().findField('P5').focus();
                T1Load();

                if (data.msg == 'Y')
                    msglabel('核撥過帳成功');
                else
                    alert(data.msg);

                var layout = viewport.down('#t1Main').getLayout();
                layout.setActiveItem(0);
            },
            failure: function (response, options) {
                myMask.hide();
                msglabel('核撥過帳失敗');
            }
        });
    }

    // 整批揀貨
    function updatePickAll() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: pickAllSet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: T1Query.getForm().findField('P5').getValue(),
                p6: T1Query.getForm().findField('P6').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                Ext.Msg.alert('訊息', '整批揀貨完成!', function () {
                    setLotNoData(T1Query.getForm().findField('P0').getValue());
                    setDocnoData(T1Query.getForm().findField('P0').getValue());
                    T1Load();
                });
            },
            failure: function (response, options) {
                myMask.hide();
                msglabel('整批揀貨失敗');
            }
        });
    }

    // 取得揀貨人員清單
    function setPickUserData(wh_no, firstTime) {
        Ext.Ajax.request({
            url: pickUserComboGet,
            params: {
                wh_no: wh_no
                // pick_date: T1Query.getForm().findField('P2').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var hasUser = false;
                T1Query.getForm().findField('P3').setValue('');
                //T1Query.getForm().findField('P4').setDisabled(true);
                //T1Query.getForm().findField('P6').setDisabled(true);
                pickUserQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            //if (tb_data[i].VALUE == session['UserId'])
                            //    hasUser = true; // 使用者有在清單中
                            pickUserQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                    else {
                        if (firstTime)
                            Ext.Msg.alert('訊息', '查不到此庫房需揀貨人員資料');
                    }

                    //if (hasUser)
                    //    T1Query.getForm().findField('P3').setValue(session['UserId']);
                    //else
                    //{
                    //    if (firstTime)
                    //    {
                    //        Ext.Msg.alert('訊息', '查不到您需要揀貨的資料');
                    //        if (tb_data.length > 0)
                    //            T1Query.getForm().findField('P3').setValue(tb_data[0].VALUE);
                    //    } 
                    //}
                    // 取揀貨人員條件放寬,現在直接預設登入者
                    T1Query.getForm().findField('P3').setValue(session['UserId']);

                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 取得揀貨批次清單
    function setLotNoData(wh_no) {
        Ext.Ajax.request({
            url: lotNoComboGet,
            params: {
                wh_no: wh_no,
                pick_date: T1Query.getForm().findField('P2').getValue(),
                pick_user: T1Query.getForm().findField('P3').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                T1Query.getForm().findField('P4').setValue('');
                lotNoQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            lotNoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 取得揀貨申請單
    function setDocnoData(wh_no) {
        Ext.Ajax.request({
            url: docnoComboGet,
            params: {
                wh_no: wh_no,
                pick_date: T1Query.getForm().findField('P2').getValue(),
                pick_user: T1Query.getForm().findField('P3').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                T1Query.getForm().findField('P6').setValue('');
                docnoQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            docnoQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
                        if (docnoScan)
                            T1Query.getForm().findField('P6').setValue(docnoScan);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 取得揀貨批次資料清單
    function setLotNoDataDetail(wh_no, mmcode) {
        Ext.Ajax.request({
            url: lotNoDataComboGet,
            params: {
                wh_no: wh_no,
                mmcode: mmcode
            },
            method: reqVal_p,
            success: function (response) {
                lotNoDataStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            lotNoDataStore.add({ WH_NO: tb_data[i].WH_NO, MMCODE: tb_data[i].MMCODE, LOT_NO: tb_data[i].LOT_NO, EXP_DATE: tb_data[i].EXP_DATE, INV_QTY: tb_data[i].INV_QTY });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 2020-11-27 取得申請單資訊
    function getDocnoInfo(docno) {
        Ext.Ajax.request({
            url: '/api/CD0004/GetDocnoInfo',
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue(),
                pick_date: T1Query.getForm().findField('P2').getValue(),
                pick_user: T1Query.getForm().findField('P3').getValue(),
                docno: docno
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var f = T1Query.getForm();
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        f.findField('P6_1').setValue(tb_data[0].APPDEPT_NAME);
                        f.findField('P2').setValue(tb_data[0].PICK_DATE);
                        T1Query.getForm().findField('P5').focus();
                    } else {
                        
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 檢查條碼
    function chkBarcode(bcode) {
        docnoScan = '';
        Ext.Ajax.request({
            url: chkBarcodeGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: bcode,
                p6: T1Query.getForm().findField('P6').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var cnt = data.msg;
                    if (cnt == '1') {
                        // 有找到院內碼,直接顯示該品項
                        showDetail = true;
                        T1Load();
                    }
                    else if (cnt == 'C') {
                        // 有找到申請單,但單據已結案
                        Ext.Msg.alert('訊息', '此申請單已結案', function () {
                            T1Query.getForm().findField('P5').setValue('');
                            popform.getForm().findField('BARCODE').setValue('');
                        });
                    }
                    else if (cnt == 'D') {
                        // 有找到申請單,帶入揀貨日期並將申請單號帶入依申請單再查詢
                        var layout = viewport.down('#t1Main').getLayout();
                        layout.setActiveItem(0);

                        docnoScan = bcode;
                        var tb_data = data.etts;
                        if (tb_data.length > 0) {
                            T1Query.getForm().findField('P2').setValue(tb_data[0].PICK_DATE);
                        }

                        var f = T1Query.getForm();
                        f.findField('P6_SEL').setValue(true);
                        //enableField('P6');
                        T1Query.getForm().findField('P6').setValue(bcode);
                        T1Query.getForm().findField('P5').setValue('');

                        setQueryFieldVisible(false);
                        showDetail = true; // 查詢申請單一樣自動顯示第一筆
                        T1Load();
                    }
                    else
                        Ext.Msg.alert('訊息', '查不到對應資料', function () {
                            T1Query.getForm().findField('P5').setValue('');
                            popform.getForm().findField('BARCODE').setValue('');
                        });
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 查詢是否申請單品項均已揀貨完成但尚未出貨(僅中央庫房使用)
    function chkPickCnt() {
        Ext.Ajax.request({
            url: chkPickCntGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p6: T1Query.getForm().findField('P6').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    if (rtnStr == 'T') {
                        // 申請單所有品項均揀貨完成但尚未出貨
                        T1Query.down('#btnShipoutAll').setDisabled(false);
                        T1Query.down('#btnShipoutAll').click();
                    }
                    else if (rtnStr == 'F') {
                        T1Query.down('#btnShipoutAll').setDisabled(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 整批出庫
    function shipoutAll() {
        Ext.Ajax.request({
            url: shipoutAllSet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p6: T1Query.getForm().findField('P6').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    msglabel('整批出庫完成');
                    T1Load();
                    T1Query.down('#btnShipoutAll').setDisabled(true);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 檢查登入者所屬庫房
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
                        T1Query.getForm().findField('P1').setValue(tb_data[0].WH_NAME);
                        setPickUserData(tb_data[0].WH_NO, true);
                        //setLotNoData(tb_data[0].WH_NO);
                        //setDocnoData(tb_data[0].WH_NO);
                        T1Grid.down('#COL_MMNAME_C').setVisible(false);
                        T1Grid.down('#COL_MMNAME_E').setVisible(true);
                        T1Query.down('#btnShipoutAll').setVisible(false);
                        T1Query.down('#btnPickAll').setVisible(false);
                    }
                    else
                        Ext.Msg.alert('訊息', '查不到您所屬庫房資料');
                }
            },
            failure: function (response, options) {

            }
        });
    }
    chkUserWhno();

    // 依傳入的欄位是P4(依分配批次)或P6(依申請單號)設定相關欄位屬性
    function enableField(fieldType) {
        if (fieldType == 'P4') {
            var f = T1Query.getForm();
            f.findField('P6_SEL').setValue(false);
            f.findField('P6').setValue('');
            f.findField('P4').setDisabled(false);
            f.findField('P2').setDisabled(false);
            f.findField('P6').setDisabled(true);
        }
        else if (fieldType == 'P6') {
            var f = T1Query.getForm();
            f.findField('P4_SEL').setValue(false);
            f.findField('P4').setValue('');
            f.findField('P4').setDisabled(true);
            f.findField('P2').setDisabled(true);
            f.findField('P6').setDisabled(false);
        }
    }

    function setQueryFieldVisible(fIsVisible) {
        var f = T1Query.getForm();
        f.findField('P0').setVisible(fIsVisible);
        f.findField('P1').setVisible(fIsVisible);
        //f.findField('P2').setVisible(fIsVisible);
        f.findField('P3').setVisible(fIsVisible);
        f.findField('P7').setVisible(fIsVisible);
        if (fIsVisible == true)
            T1Query.down('#visibleBtn').setText('<font size="2vmin">●</font>');
        else
            T1Query.down('#visibleBtn').setText('<font size="2vmin">○</font>');
        setTimeout(function () {
            T1Query.getForm().findField('P5').setValue('');
            T1Query.getForm().findField('P5').focus();
        }, 300);
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
            id: 't1Main',
            region: 'center',
            layout: 'card',
            collapsible: false,
            title: '',
            border: false,
            items: [{
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T1Grid]
            }, {
                itemId: 't2Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T2Grid]
            }, {
                itemId: 't3Form',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [popform]
            }
            ]
        }]
    });

    Ext.getDoc().dom.title = T1Name;
    T1Query.getForm().findField('P2').setValue(new Date());

    T1Query.getForm().findField('P5').focus(); // 預設focus在院內碼,方便其他條件皆可自動代入時,可直接掃條碼
});
