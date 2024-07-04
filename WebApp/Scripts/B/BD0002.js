Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BD0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "衛材訂單維護";

    var T1Rec = 0;
    var T1LastRec = null;

    var start_date = '';
    var end_date = '';
    var first_wh_no = '';
    var first_mat_class = '';

    var GetWH_NO = '../../../api/BD0002/GetWH_NO';
    var GetMAT_CLASS = '../../../api/BD0002/GetMAT_CLASS';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var WH_NO_Store = Ext.create('Ext.data.Store', {  //查詢庫房代碼的store
        fields: ['VALUE', 'TEXT']
    });
    var MAT_CLASS_Store = Ext.create('Ext.data.Store', {  //查詢物料類別的store
        fields: ['VALUE', 'TEXT']
    });
    var XACTION_Store = Ext.create('Ext.data.Store', {  //寄放責任地點的store
        fields: ['VALUE', 'TEXT']
    });

    function SetWH_NO() { //建立庫房代碼的下拉式選單
        Ext.Ajax.request({
            url: GetWH_NO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            WH_NO_Store.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                            first_wh_no = wh_nos[0].VALUE;
                        }
                    }
                    T1QueryForm.getForm().findField("P0").setValue(first_wh_no);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetMAT_CLASS() { //建立物料類別的下拉式選單
        Ext.Ajax.request({
            url: GetMAT_CLASS,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var mat_class = data.etts;
                    if (mat_class.length > 0) {
                        for (var i = 0; i < mat_class.length; i++) {
                            MAT_CLASS_Store.add({ VALUE: mat_class[i].VALUE, TEXT: mat_class[i].TEXT });
                            first_mat_class = mat_class[0].VALUE;
                        }
                    }
                    T1QueryForm.getForm().findField("P1").setValue(first_mat_class);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetXACTION() { //建立Master的狀態的下拉式選單
        XACTION_Store.add({ VALUE: '0', TEXT: '0 臨時申購' });
        XACTION_Store.add({ VALUE: '1', TEXT: '1 常態申購' });
        T1QueryForm.getForm().findField("P4").setValue('0');
    }
    
    // 查詢欄位
    var T1QueryForm = Ext.widget({
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
            labelWidth: 60,
            width: 235
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'combo',
                fieldLabel: '庫房別',
                name: 'P0',
                id: 'P0',
                store: WH_NO_Store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入庫房別"
            }, {
                xtype: 'combo',
                fieldLabel: '物料類別',
                name: 'P1',
                id: 'P1',
                store: MAT_CLASS_Store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入物料類別"
            }, {
                xtype: 'datefield',
                fieldLabel: '申購日期',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入申購日期(起)",
                value: new Date(),
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d');
                }
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'P3',
                id: 'P3',
                enforceMaxLength: true,
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入申購日期(迄)",
                value: new Date(),
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d');
                }
            }, {
                xtype: 'combo',
                fieldLabel: '採購類別',
                name: 'P4',
                id: 'P4',
                store: XACTION_Store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入採購類別"
            }],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                msglabel("");
                if (T1QueryForm.getForm().isValid()) {
                    T1Load();
                }
                else {
                    if (T1QueryForm.getForm().findField("P0").getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>為必填');
                        msglabel(" <span style='color:red'>庫房別</span>為必填");
                    }
                    else if (T1QueryForm.getForm().findField("P1").getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>物料類別</span>為必填');
                        msglabel(" <span style='color:red'>物料類別</span>為必填");
                    }
                    else if (T1QueryForm.getForm().findField("P2").getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>申購日期(起)</span>為必填');
                        msglabel(" <span style='color:red'>申購日期(起)</span>為必填");
                    }
                    else if (T1QueryForm.getForm().findField("P3").getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>申購日期(迄)</span>為必填');
                        msglabel(" <span style='color:red'>申購日期(迄)</span>為必填");
                    }
                    else if (T1QueryForm.getForm().findField("P4").getValue() == null) {
                        Ext.Msg.alert('提醒', '<span style=\'color:red\'>採購類別</span>為必填');
                        msglabel(" <span style='color:red'>採購類別</span>為必填");
                    }
                }

            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                T1QueryForm.getForm().findField("P0").setValue(first_wh_no);
                T1QueryForm.getForm().findField("P1").setValue(first_mat_class);
                T1QueryForm.getForm().findField("P2").setValue(new Date());
                T1QueryForm.getForm().findField("P3").setValue(new Date());
                T1QueryForm.getForm().findField("P4").setValue('0');
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BD0002M', { // 定義於/Scripts/app/store/BD0002M.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P5的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: T1QueryForm.getForm().findField('P2').rawValue,
                    p3: T1QueryForm.getForm().findField('P3').rawValue,
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
        items: [
            {
                xtype: 'button',
                itemId: 'create_order',
                text: '產生訂單',
                disabled: false,
                handler: function () {
                    msglabel("");
                    if (T1QueryForm.getForm().findField('P4').getValue() == '0') {
                        Ext.MessageBox.confirm('產生訂單', '確定要產生<span style=\'color:red\'>臨時申購訂單</span>？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '../../../api/BD0002/CREATE_ORDER_0';
                                T1Submit();
                            }
                        });
                    }
                    else if (T1QueryForm.getForm().findField('P4').getValue() == '1') {
                        Ext.MessageBox.confirm('產生訂單', '確定要產生<span style=\'color:red\'>常態申購訂單</span>？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '../../../api/BD0002/CREATE_ORDER_1';
                                T1Submit();
                            }
                        });
                    }
                }
            }
        ],
        emptyMsg: '沒有任何資料       <span style=\'color:red\'>   常態申購--每月22日至31日 , 訂單編號抓起始申購單日期</span>',
        displayMsg: "顯示{0} - {1}筆,共{2}筆       <span style=\'color:red\'>   常態申購--每月22日至31日 , 訂單編號抓起始申購單日期</span>",
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
        items: [{
            xtype: 'displayfield',
            fieldLabel: '採購類別', //白底顯示
            name: 'XACTION_CODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '對外採購單編號',//白底顯示
            name: 'PR_NO',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '物料類別', //白底顯示
            name: 'MAT_CLASS',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫備/非庫備',//白底顯示
            name: 'M_STOREID_CODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請狀態', //白底顯示
            name: 'PR_STATUS_CODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請日期', //白底顯示
            name: 'PR_TIME',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '緊急醫療出或', //白底顯示
            name: 'IS_CR',
            readOnly: true
        }]
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
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "採購類別",
            dataIndex: 'XACTION_NAME',
            width: 80
        }, {
            text: "對外採購單編號",
            dataIndex: 'PR_NO',
            width: 155
        }, {
            text: "物料類別",
            dataIndex: 'MAT_CLASS',
            width: 110
        }, {
            text: "庫備/非庫備",
            dataIndex: 'M_STOREID_NAME',
            width: 90
        }, {
            text: "申請狀態",
            dataIndex: 'PR_STATUS_NAME',
            width: 90
        }, {
            text: "申請日期",
            dataIndex: 'PR_TIME',
            width: 90
        }, {
            text: "緊急醫療出貨",
            dataIndex: 'IS_CR',
            width: 120
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
                        TATabs.setActiveTab('Form');
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                if (T1LastRec) {
                    msglabel("");

                }
            }
        }
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                params: {
                    WH_NO: T1QueryForm.getForm().findField('P0').getValue(),
                    MAT_CLASS: T1QueryForm.getForm().findField('P1').getValue(),
                    START_DATE: T1QueryForm.getForm().findField('P2').rawValue,
                    END_DATE: T1QueryForm.getForm().findField('P3').rawValue
                },
                success: function (form, action) {
                    myMask.hide();
                    if (form.responseText.replace(new RegExp('"', "g"), '') == '000') {
                        msglabel("已產生訂單");
                    }
                    else {
                        msglabel("訂單產生失敗");
                    }

                    TATabs.setActiveTab('Query');
                    T1LastRec = false;
                    T1Form.getForm().reset();
                    T1Load();
                    T1Cleanup();
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
            });
        }
    }

    //點選master的項目後
    function setFormT1a() {
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            var u = f.findField('PR_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            TATabs.setActiveTab('Form');
        }
        else {
            T1Form.getForm().reset();
        }

        T2Load();
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.readOnly = true;
            fc.setReadOnly(true);
        });

        //viewport.down('#form').setTitle('瀏覽');
        TATabs.down('#Form').setTitle('瀏覽');
        setFormT1a();
    }

    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var T2Store = Ext.create('WEBAPP.store.BD0002D', { // 定義於/Scripts/app/store/BD0002D.js
        listeners: {
            //載入前將master的院內碼關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("PR_NO").getValue()
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
        autoScroll: true,
        bodyPadding: '10',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90,
            width: 260
        },
        defaultType: 'textfield',
        items: [{
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
            fieldLabel: '申購單位(包裝單位)', //白底顯示
            name: 'M_PURUN',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申購數量(包裝單位)', //白底顯示
            name: 'REQ_QTY_T',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '合約價', //白底顯示
            name: 'M_CONTPRICE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '轉換率', //白底顯示
            name: 'UNIT_SWAP',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '最小單位', //白底顯示
            name: 'BASE_UNIT',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申請數量', //白底顯示
            name: 'PR_QTY',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '最小單價', //白底顯示
            name: 'PR_PRICE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商名稱', //白底顯示
            name: 'AGEN_NAME',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠商代碼', //白底顯示
            name: 'AGEN_NO',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠牌', //白底顯示
            name: 'M_AGENLAB',
            readOnly: true
        },{
            xtype: 'displayfield',
            fieldLabel: '廠商電話', //白底顯示
            name: 'AGEN_TEL',
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 200,
            sortable: true
        }, {
            text: "申購單位(包裝單位)",
            dataIndex: 'M_PURUN',
            width: 120,
            sortable: true
        }, {
            text: "申購數量(包裝單位)",
            dataIndex: 'REQ_QTY_T',
            width: 120,
            sortable: true,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "合約價",
            dataIndex: 'M_CONTPRICE',
            width: 70,
            sortable: true,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "轉換率",
            dataIndex: 'UNIT_SWAP',
            width: 70,
            sortable: true,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "最小單位",
            dataIndex: 'BASE_UNIT',
            width: 70
        }, {
            text: "申請數量",
            dataIndex: 'PR_QTY',
            width: 70,
            sortable: true,
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "最小單價",
            dataIndex: 'PR_PRICE',
            width: 70,
            sortable: true,
            //xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left'
        }, {
            text: "廠商名稱",
            dataIndex: 'AGEN_NAME',
            width: 130,
            sortable: true
        }, {
            text: "廠商代碼",
            dataIndex: 'AGEN_NO',
            width: 80,
            sortable: true
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 130,
            sortable: true
        }, {
            text: "廠商電話",
            dataIndex: 'AGEN_TEL',
            width: 80,
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
                        TATabs.setActiveTab('Form');
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
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

    //點選detail項目後
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();

            var u = f.findField('MMCODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
        }
        else {
            T2Form.getForm().reset();
        }
    }

    //view 
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1QueryForm]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
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
                        // items: [TATabs]
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
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

    SetWH_NO();
    SetMAT_CLASS();
    SetXACTION();
    //GetDate();
    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField("P2").setValue(new Date());
    T1QueryForm.getForm().findField("P3").setValue(new Date());
    T1QueryForm.getForm().findField("P4").setValue('0');
});
