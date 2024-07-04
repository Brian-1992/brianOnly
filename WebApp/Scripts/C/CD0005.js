Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/CD0005/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "揀貨確認";

    var T1Rec = 0;
    var T1LastRec = null;

    var GetWH_NO = '../../../api/CD0005/GetWH_NO';
    var GetPICK_USERID = '../../../api/CD0005/GetPICK_USERID';

    var First_WHNO = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var WH_NOStore = Ext.create('Ext.data.Store', {  //查詢庫房號碼的store
        fields: ['VALUE', 'TEXT']
    });
    var PICK_Store = Ext.create('Ext.data.Store', {  //查詢揀貨人員的store
        fields: ['VALUE', 'TEXT']
    });

    function SetWH_NO() { //建立查詢庫房號碼的下拉式選單
        Ext.Ajax.request({
            url: GetWH_NO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            WH_NOStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                            First_WHNO = wh_nos[0].VALUE;
                        }
                        T1QueryForm.getForm().findField("P0").setValue(First_WHNO);
                        SetPICK_USERID(First_WHNO, T1QueryForm.getForm().findField("P1").getValue());
                        msglabel("");
                    }
                    else {
                        Ext.Msg.alert('提醒', "查不到您所屬庫房資料");
                        msglabel("查不到您所屬庫房資料");
                        T1QueryForm.down('#query').setDisabled(true);
                        T1QueryForm.down('#clean').setDisabled(true);
                        T1QueryForm.getForm().findField("P0").clearInvalid();
                        T1QueryForm.getForm().findField("P0").setReadOnly(true);
                        T1QueryForm.getForm().findField("P0").allowBlank = true;
                        T1QueryForm.getForm().findField("P1").setValue("");
                        T1QueryForm.getForm().findField("P1").clearInvalid();
                        T1QueryForm.getForm().findField("P1").allowBlank = true;
                        T1QueryForm.getForm().findField("P1").setReadOnly(true);
                        T1QueryForm.getForm().findField("P2").setReadOnly(true);
                        T1QueryForm.getForm().findField("P3").setReadOnly(true);
                        T1QueryForm.getForm().findField("P4").setReadOnly(true);
                    }
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('錯誤', "抓取庫房資料出現錯誤");
                msglabel("抓取庫房資料出現錯誤");
            }
        });
    }

    function SetPICK_USERID(wh_no, pick_date) {
        var promise = Pick_UseridPromise(wh_no, pick_date);
        promise.then(function (success) {
            var data = JSON.parse(success);
            if (data.success) {
                PICK_Store.removeAll();
                var picks = data.etts;
                if (picks.length > 0) {
                    PICK_Store.add({ VALUE: '', TEXT: '' });
                    T1QueryForm.getForm().findField("P2").setValue('');
                    for (var i = 0; i < picks.length; i++) {
                        PICK_Store.add({ VALUE: picks[i].VALUE, TEXT: picks[i].TEXT });
                    }
                    T1QueryForm.down('#query').setDisabled(false);
                    T1QueryForm.down('#clean').setDisabled(false);
                    T1QueryForm.getForm().findField("P2").setReadOnly(false);
                    T1QueryForm.getForm().findField("P3").setReadOnly(false);
                    T1QueryForm.getForm().findField("P4").setReadOnly(false);
                    msglabel("");
                }
                else {
                    Ext.Msg.alert('提醒', "查不到此庫房分派的揀貨人員資料");
                    msglabel("查不到此庫房分派的揀貨人員資料");
                    T1QueryForm.down('#query').setDisabled(true);
                    T1QueryForm.down('#clean').setDisabled(true);
                    T1QueryForm.getForm().findField("P2").setReadOnly(true);
                    T1QueryForm.getForm().findField("P3").setReadOnly(true);
                    T1QueryForm.getForm().findField("P4").setReadOnly(true);
                    T1Store.removeAll();
                }
            }
        }
        )
    }

    function Pick_UseridPromise(wh_no, pick_date) {
        var deferred = new Ext.Deferred();

        Ext.Ajax.request({
            url: GetPICK_USERID,
            method: reqVal_p,
            params: {
                WH_NO: wh_no,
                PICK_DATE: pick_date
            },
            success: function (response) {
                deferred.resolve(response.responseText);
            },

            failure: function (response) {
                deferred.reject(response.status);
                Ext.Msg.alert('錯誤', "抓取揀貨人員資料出現錯誤");
                msglabel("抓取揀貨人員資料出現錯誤");
            }
        });

        return deferred.promise; //will return the underlying promise of deferred
    }

    // 查詢欄位
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        padding: 3,
        autoScroll: true,
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70,
            width: 180
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 70,
                    width: 230,
                    store: WH_NOStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    fieldCls: 'required',
                    allowBlank: false, // 欄位是否為必填
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            PICK_Store.removeAll();
                            var wh_no = newValue.data.VALUE;
                            var pick_date = T1QueryForm.getForm().findField("P1").getValue();
                            SetPICK_USERID(wh_no, pick_date);
                        }
                    }
                }, {
                    xtype: 'datefield',
                    fieldLabel: '揀貨日期',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 70,
                    width: 200,
                    value: new Date(),
                    enforceMaxLength: true,
                    fieldCls: 'required',
                    allowBlank: false, // 欄位是否為必填
                    blankText: "請輸入有效日期",
                    renderer: function (value, meta, record) {
                        return Ext.util.Format.date(value, 'X/m/d');
                    },
                    listeners: {
                        change: function (oldValue, newValue, eOpts) {
                            PICK_Store.removeAll();
                            if (newValue != null) {
                                if (newValue.toString().length == 7) {
                                    var pick_date = String(parseInt(newValue) + 19110000);
                                    var pick_date1 = pick_date.substr(0, 4) + '/' + pick_date.substr(4, 2) + '/' + pick_date.substr(6, 2);
                                    var wh_no = T1QueryForm.getForm().findField("P0").getValue();

                                    if (wh_no != null) {
                                        SetPICK_USERID(wh_no, pick_date1);
                                    }
                                }
                                else if (newValue.toString().length > 7) {
                                    var pick_date = String(parseInt(Ext.util.Format.date(newValue, 'Xmd')) + 19110000);
                                    var pick_date1 = pick_date.substr(0, 4) + '/' + pick_date.substr(4, 2) + '/' + pick_date.substr(6, 2);
                                    var wh_no = T1QueryForm.getForm().findField("P0").getValue();

                                    if (wh_no != null) {
                                        SetPICK_USERID(wh_no, pick_date1);
                                    }
                                }
                                else {
                                    T1QueryForm.down('#query').setDisabled(true);
                                    T1QueryForm.down('#clean').setDisabled(true);
                                    T1QueryForm.getForm().findField("P2").setReadOnly(true);
                                    T1QueryForm.getForm().findField("P3").setReadOnly(true);
                                    T1QueryForm.getForm().findField("P4").setReadOnly(true);
                                }
                            }
                        }
                    }
                }, {
                    xtype: 'combo',
                    fieldLabel: '揀貨人員',
                    name: 'P2',
                    id: 'P2',
                    labelWidth: 70,
                    width: 250,
                    store: PICK_Store,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            padding: '10 0 0 0 ',
            items: [
                {
                    xtype: 'radiogroup',
                    name: 'P3',
                    fieldLabel: '揀貨差異',
                    labelWidth: 70,
                    width: 200,
                    items: [
                        { boxLabel: '無差異', name: 'P3', inputValue: '1', width: 55, checked: true },
                        { boxLabel: '有差異', name: 'P3', inputValue: '0', width: 55 }
                    ]
                },
                {
                    xtype: 'radiogroup',
                    name: 'P4',
                    fieldLabel: '確認狀態',
                    labelWidth: 100,
                    width: 235,
                    items: [
                        { boxLabel: '已確認', name: 'P4', inputValue: '1', width: 55 },
                        { boxLabel: '待確認', name: 'P4', inputValue: '0', width: 55, checked: true }
                    ]
                },
                {
                    xtype: 'button',
                    itemId: 'query',
                    text: '查詢',
                    margin: '0 0 0 15',
                    handler: function () {
                        //viewport.down('#form').setCollapsed("true");
                        msglabel("");
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    itemId: 'clean',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        T1QueryForm.getForm().findField("P0").setValue(First_WHNO);
                        SetPICK_USERID(First_WHNO, T1QueryForm.getForm().findField("P1").getValue());
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.CD0005M', { // 定義於/Scripts/app/store/CD0005M.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P5的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p4: T1QueryForm.getForm().findField('P4').getValue()
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
                itemId: 'confirm_ok',
                text: '確認完成',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('確認完成', '是否將已點選的揀貨資料設定為已確認狀態？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/CD0005/UpdateOK';
                            T1Form.getForm().findField('x').setValue('O');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'confirm_cancel',
                text: '確認取消',
                disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('確認取消', '是否將已點選的揀貨資料設定為待確認狀態？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '../../../api/CD0005/UpdateCanael';
                            T1Form.getForm().findField('x').setValue('X');
                            T1Submit();
                        }
                    });
                }
            }]
    });

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '10',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90,
            width: 260
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                fieldLabel: '揀貨差異',
                name: 'ACT_PICK_QTY_CODE',
                xtype: 'hidden'
            }, {
                fieldLabel: '確認狀態',
                name: 'HAS_CONFIRMED',
                xtype: 'hidden'
            }, {
                fieldLabel: '庫房代碼',
                name: 'WH_NO',
                xtype: 'hidden'
            }, {
                fieldLabel: '揀貨日期',
                name: 'PICK_DATE',
                xtype: 'hidden'
            }, {
                fieldLabel: '分配揀貨人員代碼',
                name: 'PICK_USERID',
                xtype: 'hidden'
            }, {
                fieldLabel: '是否已確認揀貨結果',
                name: 'HAS_CONFIRMED_CODE',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '揀貨批次', //白底顯示
                name: 'LOT_NO',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '揀貨人員', //白底顯示
                name: 'PICK_USERNAME',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '申請單號碼', //白底顯示
                name: 'DOCNO',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '品項數', //白底顯示
                name: 'ITEM_CNT_SUM',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '總件數', //白底顯示
                name: 'APPQTY_SUM',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '已揀項數', //白底顯示
                name: 'PICK_ITEM_CNT_SUM',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '已揀總件數', //白底顯示
                name: 'ACT_PICK_QTY_SUM',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '確認狀態', //白底顯示
                name: 'CONFIRM_STATUS',
                readOnly: true
            }
        ]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            items: [T1QueryForm]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],

        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',

        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "揀貨批號",
            dataIndex: 'LOT_NO',
            width: 100
        }, {
            text: "揀貨人員名稱",
            dataIndex: 'PICK_USERNAME',
            width: 100
        }, {
            text: "申請單號碼",
            dataIndex: 'DOCNO',
            width: 180
        }, {
            text: "申請單位",
            dataIndex: 'APPDEPT',
            width: 150
        }, {
            text: "品項數",
            dataIndex: 'ITEM_CNT_SUM',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "總件數",
            dataIndex: 'APPQTY_SUM',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "已揀項數",
            dataIndex: 'PICK_ITEM_CNT_SUM',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "已揀總件數",
            dataIndex: 'ACT_PICK_QTY_SUM',
            width: 90,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "確認狀態",
            dataIndex: 'CONFIRM_STATUS',
            width: 70
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                //viewport.down('#form').expand();
                setFormT1a();

                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });

    function T1Submit() {
        var selection = T1Grid.getSelection();
        let wh_no = '';
        let pick_date = '';
        let lot_no = '';
        let docno = '';
        let pick_userid = '';
        let act_pick_qty_code = '';

        //selection.map(item => {
        //    wh_no += item.get('WH_NO') + ',';
        //    pick_date += item.get('PICK_DATE') + ',';
        //    lot_no += item.get('LOT_NO') + ',';
        //    docno += item.get('DOCNO') + ',';
        //    pick_userid += item.get('PICK_USERID') + ',';
        //    act_pick_qty_code += item.get('ACT_PICK_QTY_CODE') + ',';
        //});
        $.map(selection, function (item, key) {
            wh_no += item.get('WH_NO') + ',';
            pick_date += item.get('PICK_DATE') + ',';
            lot_no += item.get('LOT_NO') + ',';
            docno += item.get('DOCNO') + ',';
            pick_userid += item.get('PICK_USERID') + ',';
            act_pick_qty_code += item.get('ACT_PICK_QTY_CODE') + ',';
        })
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                params: {
                    WH_NO: wh_no,
                    PICK_DATE: pick_date,
                    LOT_NO: lot_no,
                    DOCNO: docno,
                    PICK_USERID: pick_userid,
                    ACT_PICK_QTY_CODE: act_pick_qty_code
                },
                //async: true,
                success: function (response) {
                    myMask.hide();
                    T1Load();

                    switch (f.findField("x").getValue()) {
                        case "O":
                            msglabel('點選的揀貨資料已設定為已確認狀態');
                            break;
                        case "X":
                            msglabel('點選的揀貨資料已設定為待確認狀態');
                            break;
                    }
                    myMask.hide();
                    //viewport.down('#form').setCollapsed("true");
                    T1LastRec = false;
                    T1Form.getForm().reset();
                    T1Load();
                    viewport.down('#t1Grid').unmask();
                },
                failure: function (form, action) {
                    myMask.hide();

                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            msglabel(" Form fields may not be submitted with invalid values");
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            msglabel(" Ajax communication failed");
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            msglabel(" " + action.result.msg);
                            break;
                    }
                }
            })
        }
    }

    //點選master的項目後
    function setFormT1a() {
        T1Grid.down('#confirm_ok').setDisabled(true);
        T1Grid.down('#confirm_cancel').setDisabled(true);
        //viewport.down('#form').expand();
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            if (T1Form.getForm().findField("HAS_CONFIRMED_CODE").getValue() == "Y") {
                T1Grid.down('#confirm_ok').setDisabled(true);
                T1Grid.down('#confirm_cancel').setDisabled(false);
            }
            else {
                T1Grid.down('#confirm_ok').setDisabled(false);
                T1Grid.down('#confirm_cancel').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
        }

        T2Load();
    }

    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var T2Store = Ext.create('WEBAPP.store.CD0005D', { // 定義於/Scripts/app/store/CD0005D.js
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("WH_NO").getValue(),
                    p1: T1Form.getForm().findField("PICK_DATE").getValue(),
                    p2: T1Form.getForm().findField("LOT_NO").getValue(),
                    p3: T1Form.getForm().findField("DOCNO").getValue(),
                    p4: T1Form.getForm().findField("PICK_USERID").getValue(),
                    p5: T1Form.getForm().findField('ACT_PICK_QTY_CODE').getValue(),
                    p6: T1Form.getForm().findField('HAS_CONFIRMED').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '10',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'displayfield',
            fieldLabel: '項次', //白底顯示
            name: 'SEQ',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '院內碼', //白底顯示
            name: 'MMCODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名', //白底顯示
            name: 'MMNAME_C',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名', //白底顯示
            name: 'MMNAME_E',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請數量', //白底顯示
            name: 'APPQTY',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '揀貨數量', //白底顯示
            name: 'ACT_PICK_QTY',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '撥補單位', //白底顯示
            name: 'BASE_UNIT',
            readOnly: true
        }]
    });

    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "項次",
            dataIndex: 'SEQ',
            width: 50,
            sortable: true
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 90,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 250,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 300,
            sortable: true
        }, {
            text: "申請數量",
            dataIndex: 'APPQTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "揀貨數量",
            dataIndex: 'ACT_PICK_QTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "撥補單位",
            dataIndex: 'BASE_UNIT',
            width: 70,
            sortable: true
        }, {
            header: "",
            flex: 1
        }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
                if (T2LastRec) {
                    msglabel("");
                }
            }
        }
    });

    //點選detail項目後
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
        }
        else {
            T2Form.getForm().reset();
        }
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        }/*,
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form, T2Form]
        }*/
        ]
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    function T2Load() {
        try {
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }

    T1QueryForm.getForm().findField('P0').focus();
    //viewport.down('#form').setCollapsed("true");
    SetWH_NO();
});
