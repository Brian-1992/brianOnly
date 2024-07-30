Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "點選申請單編號"; // 2020/2/27 藥庫與中央庫分開 藥庫:CD0004, 中央庫:CD0011
    var T2Get = '/api/CD0011/ValidAll';
    var T3Get = '/api/CD0011/PickAll';
    var actPickQtySet = '/api/CD0011/SetActPickQty';
    var pickAllSet = '/api/CD0011/SetPickAll';
    var shipoutAllSet = '/api/CD0011/SetShipoutAll';
    var chkBarcodeGet = '/api/CD0011/ChkBarcode';
    var chkMmcodeGet = '/api/CD0011/ChkMmcode';
    var chkPickCntGet = '/api/CD0011/chkPickCnt';
    var chkUserWhnoGet = '/api/CD0011/GetChkUserWhno';
    var pickUserComboGet = '/api/CD0011/GetPickUserCombo';
    var lotNoComboGet = '/api/CD0011/GetLotNoCombo';
    var docnoComboGet = '/api/CD0011/GetDocnoCombo';
    var appdeptComboGet = '/api/CD0011/GetAppdeptCombo';
    var lotNoDataComboGet = '/api/CD0011/GetLotNoDataCombo';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1LastMmcode = '';
    var T2LastRec = null;
    var T2LastRecIdx = 0;
    var showDetail = false;
    var rtnType = 0; // 0: 揀貨確認; 1: 揀貨取消(刪除)
    var isClick = false; // 是否透過Grid點選品項

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [{ name: 'WH_NO', type: 'string' },
        { name: 'PICK_DATE', type: 'string' },
        { name: 'DOCNO', type: 'string' },
        { name: 'SEQ', type: 'string' },
        { name: 'MMCODE', type: 'string' },
        { name: 'APPQTY', type: 'string' },
        { name: 'BASE_UNIT', type: 'string' },
        { name: 'APLYITEM_NOTE', type: 'string' },
        { name: 'MAT_CLASS', type: 'string' },
        { name: 'MMNAME_C', type: 'string' },
        { name: 'MMNAME_E', type: 'string' },
        { name: 'WEXP_ID', type: 'string' },
        { name: 'WEXP_ID_DESC', type: 'string' },
        { name: 'STORE_LOC', type: 'string' },
        { name: 'PICK_USERID', type: 'string' },
        { name: 'PICK_SEQ', type: 'string' },
        { name: 'ACT_PICK_USERID', type: 'string' },
        { name: 'ACT_PICK_QTY', type: 'string' },
        { name: 'ACT_PICK_TIME', type: 'string' },
        { name: 'ACT_PICK_NOTE', type: 'string' },
        { name: 'HAS_CONFIRMED', type: 'string' },
        { name: 'BOXNO', type: 'string' },
        { name: 'BARCODE', type: 'string' },
        { name: 'XCATEGORY', type: 'string' },
        { name: 'CREATE_DATE', type: 'string' },
        { name: 'CREATE_USER', type: 'string' },
        { name: 'UPDATE_DATE', type: 'string' },
        { name: 'UPDATE_USER', type: 'string' },
        { name: 'UPDATE_IP', type: 'string' },
        // ================================
        { name: 'LOT_NO', type: 'string' },
        { name: 'INV_QTY', type: 'string' },
        { name: 'APPDEPT', type: 'string' },
        { name: 'APPLY_NOTE', type: 'string' },
        { name: 'NEED_PICK_ITEMS', type: 'string' },
        { name: 'LACK_PICK_ITEMS', type: 'string' },
        { name: 'VALID_DATE', type: 'string' },
        { name: 'LOT_NO_F', type: 'string' },
        { name: 'SHIPOUTCNT', type: 'string' }]
    });
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['LOT_NO_F', 'VALID_DATE', 'ACT_PICK_QTY']
    });
    Ext.define('T3Model', {
        extend: 'Ext.data.Model',
        fields: ['WH_NO', 'PICK_DATE', 'DOCNO', 'SEQ', 'MMNAME_C', 'MMNAME_E', 'BARCODE', 'MMCODE']
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

    // 揀貨申請單位
    var appdeptQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    // 揀貨批次明細
    var lotNoDataStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'MMCODE', 'LOT_NO_F', 'VALID_DATE', 'INV_QTY']
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
        queryUrl: '/api/CD0011/GetWH_NoCombo',
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
                    setAppdeptData(r.get('WH_NO'));
                    // 若為中央庫房則顯示中文名稱,其他則顯示英文名稱
                    if (r.get('WH_NO') == '560000') {
                        T1Grid.down('#COL_MMNAME_C').setVisible(true);
                        T1Grid.down('#COL_MMNAME_E').setVisible(false);
                        T1Query.down('#btnShipoutAll').setVisible(true); // 中央庫房才會用到整批出庫
                        if (T1Query.getWidth() > 600)
                            T1Query.down('#btnPickAll').setVisible(true);
                    }
                    else {
                        T1Grid.down('#COL_MMNAME_C').setVisible(false);
                        T1Grid.down('#COL_MMNAME_E').setVisible(true);
                        T1Query.down('#btnShipoutAll').setVisible(false);
                        T1Query.down('#btnPickAll').setVisible(false);
                    }
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
                wh_NoCombo, {
                    xtype: 'displayfield',
                    fieldLabel: '庫房名稱',
                    name: 'P1',
                    id: 'P1',
                    margin: '1 0 1 0',
                    enforceMaxLength: true,
                    labelStyle: 'width: 40%'
                }, {
                    xtype: 'datefield',
                    id: 'P2',
                    name: 'P2',
                    margin: '1 0 1 0',
                    fieldLabel: '揀貨日期',
                    fieldCls: 'required',
                    allowBlank: false,
                    margin: '1 0 1 0',
                    labelStyle: 'width: 40%',
                    listeners: {
                        change: function (fiels, nValue, oValue, eOpts) {
                            var f = T1Query.getForm();
                            setPickUserData(f.findField('P0').getValue(), false);
                            //setLotNoData(f.findField('P0').getValue());
                        },
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
                            setAppdeptData(f.findField('P0').getValue());
                        }
                    }
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
                            listeners: {
                                change: function (field, nVal, oVal, eOpts) {
                                    if (nVal) {
                                        enableField('P4');
                                    }
                                }
                            }
                        }, {
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
                            labelStyle: 'width: 45%',
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
                            listeners: {
                                change: function (field, nVal, oVal, eOpts) {
                                    if (nVal) {
                                        enableField('P6');
                                    }
                                }
                            }
                        }, {
                            xtype: 'combo',
                            store: docnoQueryStore,
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            id: 'P6',
                            name: 'P6',
                            margin: '1 0 1 0',
                            fieldLabel: '依申請單號',
                            margin: '1 0 1 0',
                            disabled: true,
                            queryMode: 'local',
                            autoSelect: true,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            width: '90%',
                            labelStyle: 'width: 45%',
                            pickerOffset: [-90, 0],
                            matchFieldWidth: false,
                            listConfig: { width: '90%' },
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
                            id: 'P7_SEL',
                            name: 'P7_SEL',
                            width: '10%',
                            padding: '4% 0 0 4%',
                            checked: true,
                            listeners: {
                                change: function (field, nVal, oVal, eOpts) {
                                    if (nVal) {
                                        enableField('P7');
                                    }
                                }
                            }
                        }, {
                            xtype: 'combo',
                            store: appdeptQueryStore,
                            displayField: 'COMBITEM',
                            valueField: 'VALUE',
                            id: 'P7',
                            name: 'P7',
                            margin: '1 0 1 0',
                            fieldLabel: '依申請單位',
                            margin: '1 0 1 0',
                            // disabled: true,
                            queryMode: 'local',
                            autoSelect: true,
                            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                            width: '90%',
                            labelStyle: 'width: 45%',
                            pickerOffset: [-90, 0],
                            matchFieldWidth: false,
                            listConfig: { width: '75%' },
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
                                    if (nValue.length >= 6) {
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
                                var p7 = T1Query.getForm().findField('P7').getValue();
                                if ((p4 == '' || p4 == null) && (p6 == '' || p6 == null) && (p7 == '' || p7 == null)) {
                                    Ext.Msg.alert('提醒', '分配批次,申請單號或申請單位至少需填寫一個');
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
                            T1Query.down('#btnShipoutAll').setDisabled(true);
                            var f = this.up('form').getForm();
                            //f.reset();
                            f.findField('P4').setValue('');
                            f.findField('P6').setValue('');
                            f.findField('P7').setValue('');
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
                            var f = T1Query.getForm();
                            if ((f.findField('P4').getValue() != '' && f.findField('P4').getValue() != null)
                                || (f.findField('P6').getValue() != '' && f.findField('P6').getValue() != null)
                                || (f.findField('P7').getValue() != '' && f.findField('P7').getValue() != null)) {

                                var diffCnt = 0; // 有差異品項計數
                                for (var i = 0; i < T1Store.data.length; i++) {
                                    if (T1Store.data.items[i].data['ACT_PICK_QTY'] != T1Store.data.items[i].data['APPQTY']) {
                                        diffCnt += 1;
                                    }
                                }

                                var diffMsg = '';
                                if (diffCnt > 0)
                                    diffMsg = '有' + diffCnt + '個品項有差異,是否仍要全部設定出庫?';
                                else
                                    diffMsg = '確定將已揀貨品項全部設定出庫?';

                                Ext.MessageBox.confirm('提醒', diffMsg, function (btn, text) {
                                    if (btn === 'yes') {
                                        shipoutAll();
                                    }
                                });
                            }
                            else
                                Ext.Msg.alert('訊息', '分配批次,申請單號或申請單位至少需填寫一個');                            
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
                popItemDetail('U');
            }
        }]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 200,
        remoteSort: true,
        sorters: [{ property: 'ACT_PICK_USERID', direction: 'DESC' },
            { property: 'APPDEPT', direction: 'ASC' },
            { property: 'STORE_LOC', direction: 'ASC' },
            { property: 'DOCNO', direction: 'ASC' },
            { property: 'SEQ', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/CD0011/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    // p5: T1Query.getForm().findField('P5').getValue(),
                    p5: '', // 不再使用條碼欄篩選清單資料
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
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
                    if (T1Query.getForm().findField('P0').getValue() == '560000') {
                        //// 若為中央庫房, 則不自動跳至下一筆,而是focus到院內碼以便掃描下一筆資料
                        //showNextRec = false;
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                        //T1Query.getForm().findField('P5').focus();

                        // 2020/3/3中央庫房也改為與藥庫相同處理方式
                        T1LastRec = records[0];
                        if (T1LastRec && showDetail && !(T1LastRec.data['ACT_PICK_QTY'])) {
                            T1Rec = 1;
                            popItemDetail('U');
                            showDetail = false;
                        }
                        else
                            showNextRec = true;

                        chkPickCnt();
                    }

                    // 顯示下一筆尚未揀貨品項
                    if (showNextRec) {
                        var getFirstRec = false;
                        var chkIdx = 0; // 本次填寫的資料索引
                        if (T1LastMmcode && rtnType == 0) // 已填寫過一筆揀貨且不是做揀貨取消
                        {
                            // 重新載入後,剛才的已揀之後會被排序到後面,這裡一律從0開始找
                            //chkIdx = store.findExact('MMCODE', T1LastMmcode);
                            chkIdx = 0;
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
        sorters: [{ property: 'MMNAME_C', direction: 'ASC' }],
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1LastRec.data['WH_NO'],
                    p1: T1LastRec.data['PICK_DATE'],
                    p2: T1LastRec.data['DOCNO'],
                    p3: T1Query.getForm().findField('P3').getValue()
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
                        if (record.data['APPQTY'] != record.data['ACT_PICK_QTY'])
                            return '<font color="blue">' + val + '</font>';
                        else
                            return '<font color="red">' + val + '</font>';
                    }
                    else
                        return val;
                }
            }, {
                text: "揀貨量",
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
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: '40%',
                plugins: ''
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: '40%'
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

    var callableWin = null;
    popItemDetail = function (setType) {
        if (!callableWin) {
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
                        layout: 'vbox',
                        height: '85%',
                        padding: '2vmin',
                        border: true,
                        style: 'border: 1px solid gray',
                        scrollable: true,
                        items: [
                            {
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
                                xtype: 'panel',
                                layout: 'hbox',
                                border: false,
                                width: '100%',
                                items: [{
                                    xtype: 'displayfield',
                                    fieldLabel: '核撥',
                                    name: 'APPQTY',
                                    readOnly: true,
                                    width: '45%',
                                    labelStyle: 'width: 50%'
                                }, {
                                    xtype: 'textfield',
                                    fieldLabel: '撥發',
                                    name: 'ACT_PICK_QTY',
                                    readOnly: false,
                                    maskRe: /[0-9.]/,
                                    regexText: '只能輸入數字',
                                    regex: /^([1-9]\d*(\.\d*)?|0)$/,
                                    width: '45%',
                                    labelStyle: 'width: 50%',
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
                                }]
                            }, {
                                xtype: 'combo',
                                fieldLabel: '批號',
                                name: 'LOT_NO_F',
                                store: lotNoDataStore,
                                displayField: 'LOT_NO_F',
                                valueField: 'LOT_NO_F',
                                queryMode: 'local',
                                fieldCls: 'required',
                                autoSelect: true,
                                editable: true, typeAhead: true, selectOnFocus: true,
                                plugins: 'responsive',
                                responsiveConfig: {
                                    'width < 600': {
                                        labelStyle: 'width: 40%',
                                    },
                                    'width >= 600': {
                                        labelStyle: 'width: 35%',
                                    }
                                },
                                listeners: {
                                    focus: function (field, event, eOpts) {
                                        if (setType != 'D') {
                                            if (!field.isExpanded) {
                                                setTimeout(function () {
                                                    field.expand();
                                                }, 300);
                                            }
                                        }
                                    },
                                    select: function (field, record, eOpts) {
                                        if (field.getValue() != '' && field.getValue() != null && field.readOnly == false) {
                                            var f_lot_data = lotNoDataStore.findRecord('LOT_NO_F', field.getValue());
                                            popform.getForm().findField('VALID_DATE').setValue(f_lot_data.data['VALID_DATE']);
                                        }
                                    }
                                }
                            }, {
                                xtype: "datefield",
                                fieldLabel: '效期',
                                name: 'VALID_DATE',
                                enforceMaxLength: true, //最多輸入的長度有限制
                                maxLength: 7,
                                fieldCls: 'required',
                                minValue: new Date,
                                maskRe: /[0-9]/,
                                listeners: {
                                    focus: function (field, event, eOpts) {
                                        if (setType != 'D') {
                                            if (!field.isExpanded) {
                                                setTimeout(function () {
                                                    field.expand();
                                                }, 300);
                                            }
                                        }
                                    },
                                    //blur: function (field, eOpts) {
                                    //    field.rawDate = field.rawValue;
                                    //}
                                },
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
                                        if (nValue.length >= 6) {
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
                                                            popform.getForm().findField('ACT_PICK_QTY').focus();
                                                            msglabel('掃描品項與目前品項相符');
                                                        }
                                                        else
                                                            Ext.Msg.alert('訊息', '掃描的條碼與目前品項不符!', function () { popform.getForm().findField('BARCODE').setValue(''); });
                                                    }
                                                },
                                                failure: function (response, options) {

                                                }
                                            });
                                        }
                                    }
                                }
                            }, {
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
                                xtype: 'displayfield',
                                fieldLabel: '申請庫別',
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
                                xtype: 'displayfield',
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
                                width: '30%',
                                labelStyle: 'width: 70%'
                            }, {
                                xtype: 'displayfield',
                                fieldLabel: '未撥',
                                name: 'LACK_PICK_ITEMS',
                                readOnly: true,
                                width: '30%',
                                labelStyle: 'width: 70%'
                            }, {
                                xtype: 'button',
                                text: '未撥品項',
                                handler: function () {
                                    var layout = viewport.down('#t1Main').getLayout();
                                    layout.setActiveItem(1);
                                    callableWin.destroy();
                                    callableWin = null;
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
                        if (popform.getForm().isValid())
                        {
                            setTimeout(function () {
                                var appQty = popform.getForm().findField('APPQTY').getValue();
                                var actPickQty = popform.getForm().findField('ACT_PICK_QTY').getValue();
                                var actPickNote = popform.getForm().findField('ACT_PICK_NOTE').getValue();
                                var lotNo = popform.getForm().findField('LOT_NO_F').getValue();
                                var validDate = popform.getForm().findField('VALID_DATE').getValue();
                                // var totalActPickQty = 0;
                                //// 若屬於效期管制項目,則統計各效期數量合計是否等於揀貨數量
                                //if (T1LastRec.data['WEXP_ID'] == 'Y') {
                                //    for (var i = 0; i < T2Store.data.length; i++) {
                                //        // 若任一項目還有欄位未填寫
                                //        if (T2Store.data.items[i].data['VALID_DATE'] == '' || T2Store.data.items[i].data['ACT_PICK_QTY'] == '') {
                                //            Ext.Msg.alert('訊息', '尚有效期項目未填入有效日期或揀貨數量!');
                                //            return false;
                                //        }
                                //        // 逐項累加各效期項目揀貨數量
                                //        totalActPickQty += parseFloat(T2Store.data.items[i].data['ACT_PICK_QTY']);
                                //    }
                                //    if (totalActPickQty != actPickQty) {
                                //        Ext.Msg.alert('訊息', '各效期數量合計與揀貨數量不同');
                                //        return false;
                                //    }
                                //}

                                if (T1LastRec.data['WEXP_ID'] == 'Y') {
                                    if (lotNo == '' || lotNo == null || validDate == '' || validDate == null) {
                                        Ext.Msg.alert('訊息', '批號及效期為必填');
                                        return false;
                                    }
                                }

                                rtnType = 0;
                                if (actPickQty > 0 && actPickQty != appQty) {
                                    Ext.MessageBox.confirm('提醒', '您輸入的揀貨量與核撥量不同</br>，是否確定要儲存資料?', function (btn, text) {
                                        if (btn === 'yes') {
                                            updateQty(actPickQty, actPickNote, 'U', lotNo, validDate);
                                        }
                                    });
                                }
                                else
                                    updateQty(actPickQty, actPickNote, 'U', lotNo, validDate);
                            }, 100); // 給予一點延遲,避免操作上在填寫一筆資料的揀貨數量後,還沒blur就直接按確定,造成store還沒更新就開始統計ACT_PICK_QTY
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
                        this.up('window').destroy();
                        callableWin = null;
                        T1Query.getForm().findField('P5').setValue('');
                        T1Query.getForm().findField('P5').focus();
                        msglabel('');
                    }
                }]
            });

            callableWin = GetPopWinMobile(viewport, popform, '揀貨內容確認', viewport.width * 0.95, viewport.height * 0.95);
        }
        callableWin.show();

        setLotNoDataDetail(T1LastRec.data['WH_NO'], T1LastRec.data['MMCODE']);
        popform.loadRecord(T1LastRec);
        // 若撥發量尚未填寫則預設代核撥量
        if (!(popform.getForm().findField('ACT_PICK_QTY').getValue()))
            popform.getForm().findField('ACT_PICK_QTY').setValue(popform.getForm().findField('APPQTY').getValue());

        // T2Store.removeAll();
        //if (T1LastRec.data['WEXP_ID'] == 'Y')
        //{
        //    T2Load();
        //    popform.down('#validGrid').setVisible(true);
        //}
        //else
        //    popform.down('#validGrid').setVisible(false);

        if (T1LastRec.data['WEXP_ID'] == 'Y') {
            popform.getForm().findField('LOT_NO_F').setVisible(true);
            popform.getForm().findField('VALID_DATE').setVisible(true);
        }
        else {
            popform.getForm().findField('LOT_NO_F').setVisible(false);
            popform.getForm().findField('VALID_DATE').setVisible(false);
        }

        // 彈出明細視窗時,若已有揀貨量則提示是否要清除之前的揀貨量
        //if (setType == 'D')
        //{
        //    Ext.MessageBox.confirm('提醒', '您之前已有揀過此項目，</br>是否要刪除之前填寫的揀貨量?', function (btn, text) {
        //        if (btn === 'yes') {
        //            rtnType = 1;
        //            updateQty(0, '', 'D');
        //        }
        //    });
        //}
        // 檢視明細模式
        if (setType == 'D') {
            popform.down('#confirmsubmit').setDisabled(true);
            popform.getForm().findField('ACT_PICK_QTY').setReadOnly(true);
            popform.getForm().findField('ACT_PICK_NOTE').setReadOnly(true);
            popform.getForm().findField('LOT_NO_F').setReadOnly(true);
            popform.getForm().findField('VALID_DATE').setReadOnly(true);
            //if (T1LastRec.data['WEXP_ID'] == 'Y')
            //{
            //    popform.down('#btnAdd').setDisabled(true);
            //    popform.down('#btnDel').setDisabled(true);
            //}
        }
        else // 編輯模式
        {
            popform.down('#confirmsubmit').setDisabled(false);
            popform.getForm().findField('ACT_PICK_QTY').setReadOnly(false);
            popform.getForm().findField('ACT_PICK_NOTE').setReadOnly(false);
            popform.getForm().findField('LOT_NO_F').setReadOnly(false);
            popform.getForm().findField('VALID_DATE').setReadOnly(false);
            //if (T1LastRec.data['WEXP_ID'] == 'Y') {
            //    popform.down('#btnAdd').setDisabled(false);
            //    popform.down('#btnDel').setDisabled(false);
            //}
        }

        // 中央庫則顯示中文名稱,其他顯示英文名稱
        if (T1Query.getForm().findField('P0').getValue() == '560000') {
            popform.getForm().findField('MMNAME_C').setVisible(true);
            popform.getForm().findField('MMNAME_E').setVisible(false);
        }
        else {
            popform.getForm().findField('MMNAME_C').setVisible(false);
            popform.getForm().findField('MMNAME_E').setVisible(true);
        }

        setTimeout(function () {
            if (T1Query.getForm().findField('P0').getValue() == '560000')
                popform.getForm().findField('BARCODE').focus();
            else {
                if (isClick) {
                    popform.getForm().findField('BARCODE').focus(); // 透過點選清單方式進入品項,則需先掃描條碼
                    msglabel('請先掃描一次條碼');
                }
                else
                    popform.getForm().findField('INV_QTY').focus();
            }
            isClick = false;
        }, 500);
    }

    // 更新或刪除揀貨量
    function updateQty(actPickQty, actPickNote, setType, setLotNo, setValidDate) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        //var records = T2Store.getRange();
        //var submitS = ''; // 單筆資料
        //var submitT = ''; // 全部資料
        //if (records.length > 0) {
        //    for (var i = 0; i < records.length; i++) {
        //        submitS = records[i].data.LOT_NO + "^" + records[i].data.VALID_DATE + "^" + records[i].data.ACT_PICK_QTY;
        //        if (i == records.length - 1)
        //            submitT += submitS;
        //        else
        //            submitT += submitS + "ˋ";
        //    }
        //}
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
                lot_no_f: setLotNo,
                valid_date: setValidDate,
                setType: setType
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                T1LastMmcode = T1LastRec.data['MMCODE'];
                // 資料更新後重載並關閉視窗
                T1Query.getForm().findField('P5').setValue('');
                T1Query.getForm().findField('P5').focus();
                T1Load();

                if (data.msg == 'Y')
                    msglabel('核撥過帳成功');
                else
                    alert(data.msg);

                callableWin.destroy();
                callableWin = null;
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
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                myMask.hide();
                Ext.Msg.alert('訊息', '整批揀貨完成!', function () {
                    //setLotNoData(T1Query.getForm().findField('P0').getValue());
                    //setDocnoData(T1Query.getForm().findField('P0').getValue());
                    //setAppdeptData(T1Query.getForm().findField('P0').getValue());
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
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    // 取得揀貨申請單位
    function setAppdeptData(wh_no) {
        Ext.Ajax.request({
            url: appdeptComboGet,
            params: {
                wh_no: wh_no,
                pick_date: T1Query.getForm().findField('P2').getValue(),
                pick_user: T1Query.getForm().findField('P3').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                T1Query.getForm().findField('P7').setValue('');
                appdeptQueryStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_data = data.etts;
                    if (tb_data.length > 0) {
                        for (var i = 0; i < tb_data.length; i++) {
                            appdeptQueryStore.add({ VALUE: tb_data[i].VALUE, COMBITEM: tb_data[i].COMBITEM });
                        }
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
                            lotNoDataStore.add({ WH_NO: tb_data[i].WH_NO, MMCODE: tb_data[i].MMCODE, LOT_NO_F: tb_data[i].LOT_NO, VALID_DATE: tb_data[i].EXP_DATE, INV_QTY: tb_data[i].INV_QTY });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 檢查條碼
    function chkBarcode(bcode) {
        Ext.Ajax.request({
            url: chkBarcodeGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: bcode,
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnMsg = data.msg;
                    if (rtnMsg == 'D') {
                        // 有找到申請單,將申請單號帶入依申請單再查詢
                        T1Query.getForm().findField('P6').setValue(bcode);
                        enableField('P6');
                        T1Query.getForm().findField('P5').setValue('');
                        setQueryFieldVisible(false);
                        showDetail = true;
                        T1Load();
                    }
                    else if (rtnMsg == 'A') {
                        // 有找到申請單位,將申請單位帶入再依申請單位查詢
                        T1Query.getForm().findField('P7').setValue(bcode);
                        enableField('P7');
                        T1Query.getForm().findField('P5').setValue('');
                        setQueryFieldVisible(false);
                        showDetail = true;
                        T1Load();
                    }
                    else if (rtnMsg.length > 1) {
                        // 有找到院內碼,直接顯示該品項
                        // showDetail = true;
                        // T1Load();
                        var bCodeChkIdx = T1Store.findExact('MMCODE', rtnMsg);
                        if (bCodeChkIdx >= 0) {
                            T1LastRec = T1Store.data.items[bCodeChkIdx];
                            popItemDetail('U');
                        }
                        else
                            msglabel('查無指定的品項');
                    }
                    else
                        Ext.Msg.alert('訊息', '查不到對應資料', function () { T1Query.getForm().findField('P5').setValue(''); });
                }
            },
            failure: function (response, options) {

            }
        });
    }

    // 查詢是否品項均已揀貨完成但尚未出貨
    // 可分別依分配批次,申請單號,申請單位檢查
    function chkPickCnt() {
        Ext.Ajax.request({
            url: chkPickCntGet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var rtnStr = data.msg;
                    if (rtnStr == 'T') {
                        // 所有品項均揀貨完成但尚未出貨
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

    // 整批出庫(可依 分配批次/申請單號/申請單位)
    function shipoutAll() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: shipoutAllSet,
            params: {
                p0: T1Query.getForm().findField('P0').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p6: T1Query.getForm().findField('P6').getValue(),
                p7: T1Query.getForm().findField('P7').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    var rtnStr = data.msg;
                    msglabel('整批出庫完成');
                    //T1Load();
                    T1Query.down('#btnShipoutAll').setDisabled(true);
                    setLotNoData(T1Query.getForm().findField('P0').getValue());
                    setDocnoData(T1Query.getForm().findField('P0').getValue());
                    setAppdeptData(T1Query.getForm().findField('P0').getValue());
                    T1Store.removeAll();
                }
            },
            failure: function (response, options) {
                myMask.hide();
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
                        //setAppdeptData(tb_data[0].WH_NO);
                        // 若為中央庫房則顯示中文名稱,其他則顯示英文名稱
                        if (tb_data[0].WH_NO == '560000') {
                            T1Grid.down('#COL_MMNAME_C').setVisible(true);
                            T1Grid.down('#COL_MMNAME_E').setVisible(false);
                            T1Query.down('#btnShipoutAll').setVisible(true);
                            if (T1Query.getWidth() > 600)
                                T1Query.down('#btnPickAll').setVisible(true);
                        }
                        else {
                            T1Grid.down('#COL_MMNAME_C').setVisible(false);
                            T1Grid.down('#COL_MMNAME_E').setVisible(true);
                            T1Query.down('#btnShipoutAll').setVisible(false);
                            T1Query.down('#btnPickAll').setVisible(false);
                        }
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

    function enableField(fieldType) {
        if (fieldType == 'P4') {
            var f = T1Query.getForm();
            f.findField('P4_SEL').setValue(true);
            f.findField('P6_SEL').setValue(false);
            f.findField('P7_SEL').setValue(false);
            f.findField('P6').setValue('');
            f.findField('P7').setValue('');
            f.findField('P4').setDisabled(false);
            f.findField('P6').setDisabled(true);
            f.findField('P7').setDisabled(true);
        }
        else if (fieldType == 'P6') {
            var f = T1Query.getForm();
            f.findField('P4_SEL').setValue(false);
            f.findField('P6_SEL').setValue(true);
            f.findField('P7_SEL').setValue(false);
            f.findField('P4').setValue('');
            f.findField('P7').setValue('');
            f.findField('P4').setDisabled(true);
            f.findField('P6').setDisabled(false);
            f.findField('P7').setDisabled(true);
        }
        else if (fieldType == 'P7') {
            var f = T1Query.getForm();
            f.findField('P4_SEL').setValue(false);
            f.findField('P6_SEL').setValue(false);
            f.findField('P7_SEL').setValue(true);
            f.findField('P4').setValue('');
            f.findField('P6').setValue('');
            f.findField('P4').setDisabled(true);
            f.findField('P6').setDisabled(true);
            f.findField('P7').setDisabled(false);
        }
    }

    function setQueryFieldVisible(fIsVisible) {
        var f = T1Query.getForm();
        f.findField('P0').setVisible(fIsVisible);
        f.findField('P1').setVisible(fIsVisible);
        f.findField('P2').setVisible(fIsVisible);
        f.findField('P3').setVisible(fIsVisible);
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
            }
            ]
        }]
    });

    Ext.getDoc().dom.title = T1Name;
    T1Query.getForm().findField('P2').setValue(new Date());

    T1Query.getForm().findField('P5').focus(); // 預設focus在院內碼,方便其他條件皆可自動代入時,可直接掃條碼
});
