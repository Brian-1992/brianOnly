Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.define('overrides.selection.CheckboxModel', {
    override: 'Ext.selection.CheckboxModel',

    getHeaderConfig: function () {
        var config = this.callParent();
        if (Ext.isFunction(this.selectable)) {
            config.selectable = this.selectable;
            config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                if (this.selectable(record)) {
                    record.selectable = false;
                    return '';
                }
                record.selectable = true;
                return this.defaultRenderer();
            };
            this.on('beforeselect', function (rowModel, record, index, eOpts) {
                return !this.selectable(record);
            }, this);
        }
        return config;
    },

    updateHeaderState: function () {
        // check to see if all records are selected
        var me = this,
            store = me.store,
            storeCount = store.getCount(),
            views = me.views,
            hdSelectStatus = false,
            selectedCount = 0,
            selected, len, i, notSelectableRowsCount = 0;

        if (!store.isBufferedStore && storeCount > 0) {
            hdSelectStatus = true;
            store.each(function (record) {
                if (!record.selectable) {
                    notSelectableRowsCount++;
                }
            }, this);
            selected = me.selected;

            for (i = 0, len = selected.getCount(); i < len; ++i) {
                if (store.indexOfId(selected.getAt(i).id) > -1) {
                    ++selectedCount;
                }
            }
            hdSelectStatus = storeCount === selectedCount + notSelectableRowsCount;
        }

        if (views && views.length) {
            me.column.setHeaderStatus(hdSelectStatus);
        }
    }
});

Ext.onReady(function () {
    // var T1Get = '/api/BH0003/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "寄售品基本資料";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1GetExcel = '/api/BH0003/Excel';

    var UserAgen_no = '';

    var GetAGEN_NO = '../../../api/BH0003/GetAGEN_NO';
    var GetMMCODE = '../../../api/BH0003/GetMMCODE';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var AGEN_NO_QStore = Ext.create('Ext.data.Store', {  //查詢廠商碼的store
        fields: ['VALUE', 'TEXT']
    });

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        id: 'P1',
        fieldLabel: '院內碼',
        allowBlank: true,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BH0003/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1QueryForm.getForm();
                if (r.get('P1') !== '') {
                    f.findField("P1").setValue(r.get('MMCODE'));
                    f.findField("P2").setValue(r.get('MMNAME_C'));
                    f.findField("P3").setValue(r.get('MMNAME_E'));
                }
            }
        }
    });

    function SetAGEN_NO_Q() { //建立查詢廠商碼的下拉式選單
        Ext.Ajax.request({
            url: GetAGEN_NO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                UserAgen_no = response.responseText.substring(1, response.responseText.length - 1);
                T1QueryForm.getForm().findField("P0").setValue(UserAgen_no);
            },
            failure: function (response, options) {

            }
        });
    }

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T1QueryForm.getForm().findField("P1").setValue(args.MMCODE);
        }
    }

    function setMmcode_i(args) {
        if (args.MMCODE !== '') {
            T1Form.getForm().findField("MMCODE").setValue(args.MMCODE);
        }
    }

    // 查詢欄位
    var T1QueryForm = Ext.widget({
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
            labelWidth: 60,
            width: 250
        },
        items: [
            {
                xtype: 'container',
                defaultType: 'textfield',
                items: [{
                    xtype: 'displayfield',
                    fieldLabel: '廠商碼',
                    name: 'P0',
                    id: 'P0'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13 // 可輸入最大長度為200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '中文品名',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 200 // 可輸入最大長度為200
                }, {
                    xtype: 'textfield',
                    fieldLabel: '英文品名',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 200 // 可輸入最大長度為200
                }]
            }],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                msglabel("訊息區:");
                T1Load();
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                T1QueryForm.getForm().findField("P0").setValue(UserAgen_no);
                f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.WB_PUT_M', { // 定義於/Scripts/app/store/WB_PUT_M.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P5的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue()
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
        plain: true
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
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 80
        },
        items: [
            {
                xtype: 'container',
                defaultType: 'textfield',
                items: [{
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                }, {
                    fieldLabel: '廠商碼',
                    name: 'AGEN_NO',
                    xtype: 'hidden'
                }, {
                    fieldLabel: '院內碼',
                    name: 'MMCODE',
                    xtype: 'hidden'
                }, {
                    fieldLabel: '現有寄放量',
                    name: 'QTY',
                    xtype: 'hidden'
                }, {
                    fieldLabel: '責任中心',
                    name: 'DEPT',
                    xtype: 'hidden'
                },/* {
                    xtype: 'displayfield',
                    fieldLabel: '廠商碼', //白底顯示
                    name: 'AGEN_NAMEC_DISPLAY',
                    readOnly: true
                },*/ {
                    xtype: 'displayfield',
                    fieldLabel: '院內碼', //白底顯示
                    name: 'MMCODE_DISPLAY',
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
                    fieldLabel: '寄放責任中心',//白底顯示
                    name: 'DEPT_NAME_TEXT',
                    readOnly: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '寄放地點', //白底顯示
                    name: 'DEPTNAME',
                    readOnly: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '現有寄放量', //白底顯示
                    name: 'QTY_DISPLAY',
                    readOnly: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '備註', //白底顯示
                    name: 'MEMO',
                    readOnly: true
                }, {

                    xtype: 'displayfield',
                    fieldLabel: '狀態', //白底顯示
                    name: 'STATUS_NAME',
                    readOnly: true
                }, {
                    fieldLabel: '狀態',
                    name: 'STATUS',
                    xtype: 'hidden'
                }]
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
        },/* {
            text: "廠商碼",
            dataIndex: 'AGEN_NAMEC',
            width: 180
        },*/ {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 300
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 400
        }, {
            text: "寄放地點",
            dataIndex: 'DEPT_NAME',
            width: 150
        }, {
            text: "現有寄放量",
            dataIndex: 'QTY',
            width: 90,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 130
        }, {
            text: "狀態",
            dataIndex: 'STATUS_NAME',
            width: 60
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
                    msglabel("訊息區:");

                }
            }
        }
    });

    //點選master的項目後
    function setFormT1a() {
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);

            TATabs.setActiveTab('Form');
            if (T1Form.getForm().findField("STATUS").getValue() == "D") {
                T2Grid.down('#add').setDisabled(T1Rec === 1);
            }
            else {
                T2Grid.down('#add').setDisabled(T1Rec === 0);
            }
            T2Grid.down('#edit').setDisabled(true);
            T2Grid.down('#confirm_return').setDisabled(true);
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
    var CHAR_TXTDAY = '';

    var EXTYPE_Store = Ext.create('Ext.data.Store', {  //異動類別的store
        fields: ['VALUE', 'TEXT']
    });
    var STATUS_DStore = Ext.create('Ext.data.Store', {  //Detail狀態的store
        fields: ['VALUE', 'TEXT']
    });

    function SetEXTYPE() { //建立異動類別的下拉式選單
        EXTYPE_Store.add({ VALUE: '10', TEXT: '10 取回' });
        EXTYPE_Store.add({ VALUE: '31', TEXT: '31 寄放' });
    }
    function SetSTATUS_D() { //建立異動類別的下拉式選單
        STATUS_DStore.add({ VALUE: 'A', TEXT: 'A 未處理' });
        STATUS_DStore.add({ VALUE: 'B', TEXT: 'B 已轉檔' });
    }

    var T2Store = Ext.create('WEBAPP.store.WB_PUT_D', { // 定義於/Scripts/app/store/WB_PUT_D.js
        listeners: {
            //載入前將master的院內碼關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("AGEN_NO").getValue(),
                    p1: T1Form.getForm().findField("MMCODE").getValue()
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
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: true, handler: function () {
                    T2Set = '../../../api/BH0003/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/BH0003/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'confirm_return', text: '確認回傳', disabled: true, handler: function () {
                    T2Set = '../../../api/BH0003/CONFIRM_RETURN';
                    Ext.MessageBox.confirm('確認回傳', '是否確認將資料回傳三總?', function (btn, text) {
                        if (btn === 'yes') {
                            Confirm_Return();
                        }
                    });
                }
            }
        ]
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '交易流水號',
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            fieldLabel: '廠商碼',
            name: 'AGEN_NO',
            xtype: 'hidden',
            submitValue: true
        }, {
            fieldLabel: '院內碼',
            name: 'MMCODE',
            xtype: 'hidden',
            submitValue: true
        }, {
            fieldLabel: '責任中心',
            name: 'DEPT',
            xtype: 'hidden',
            submitValue: true
        }, {
            fieldLabel: '交易日期',
            name: 'TXTDAY',
            xtype: 'datefield',
            readOnly: true,
            allowBlank: true,
            blankText: "請選擇交易日期",
            fieldCls: 'required',
            renderer: function (value, meta, record) {
                return Ext.util.Format.date(value, 'Xmd His');
            }
        }, {
            fieldLabel: '交易日期',//紅底顯示
            name: 'TXTDAY_TEXT',
            readOnly: true,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '交易日期', //白底顯示
            name: 'TXTDAY_DISPLAY',
            readOnly: true
        }, {
            xtype: 'combo',
            store: EXTYPE_Store,
            fieldLabel: '異動類別',
            name: 'EXTYPE',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            autoSelect: true,
            multiSelect: false,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇異動類別",
            allowBlank: false,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            readOnly: true,
            fieldCls: 'required'
        }, {
            fieldLabel: '異動類別',//紅底顯示
            name: 'EXTYPE_TEXT',
            readOnly: true,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '異動類別',//白底顯示
            name: 'EXTYPE_DISPLAY',
            readOnly: true
        }, {
            xtype: "numberfield",
            fieldLabel: '異動數量',
            name: 'QTY',
            enforceMaxLength: true,
            maxLength: 15,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            hideTrigger: true,
            minValue: 1,
            minText: "異動數量需大於0",
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '異動數量', //白底顯示
            name: 'QTY_DISPLAY',
            readOnly: true
        }, {
            xtype: 'textarea',
            scrollable: true,
            fieldLabel: '備註',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true,
            allowBlank: true // 欄位為非必填
        }, {
            xtype: 'combo',
            store: STATUS_DStore,
            fieldLabel: '狀態',
            name: 'STATUS',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            anyMatch: true,
            autoSelect: true,
            multiSelect: false,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇狀態",
            allowBlank: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            readOnly: true,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',//白底顯示
            name: 'STATUS_TEXT',
            readOnly: true
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("EXTYPE").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>異動類別不可為空</span>，請重新輸入。");
                    msglabel("訊息區: <span style='color:red'>異動類別不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("QTY").getValue()) < 1) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>異動數量需大於等於1</span>，請重新輸入。");
                    msglabel("訊息區: <span style='color:red'>異動數量需大於等於11</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("STATUS").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>狀態不可為空</span>，請重新輸入。");
                    msglabel("訊息區: <span style='color:red'>狀態不可為空</span>，請重新輸入。");
                }
                else {
                    if (this.up('form').getForm().isValid()) { // 檢查T2Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel("訊息區: 輸入資料格式有誤");
                    }
                }
            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                msglabel("訊息區:");
            }
        }]
    });

    //按了新增/刪除/修改後
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        TATabs.setActiveTab('Form');
        TATabs.down('#Form').setTitle(t);
        viewport.down('#form').expand();
        T1Form.setVisible(false);
        T2Form.setVisible(true);
        var f2 = T2Form.getForm();
        msglabel("訊息區:");
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('WEBAPP.model.WB_PUT_D'); //Scripts / app / model / WB_PUT_D.js
            T2Form.loadRecord(r);

            f2.findField("AGEN_NO").setValue(T1Form.getForm().findField("AGEN_NO").getValue());
            f2.findField("MMCODE").setValue(T1Form.getForm().findField("MMCODE").getValue());
            f2.findField("DEPT").setValue(T1Form.getForm().findField("DEPT").getValue());
            u = f2.findField("TXTDAY");
            u.setReadOnly(false);
            f2.findField("QTY").setReadOnly(false);
            f2.findField("STATUS").setValue("A");
            f2.findField("STATUS_TEXT").setValue("A 未處理");

            f2.findField("EXTYPE").setValue("10");
            setCmpShowCondition_D(true, false, false, true, false, false, true, false, false, true);
        }
        else {

            u = f2.findField('EXTYPE');
            msglabel("訊息區: <span style='color:red'>注意! 已存在資料不能更改[異動數量],請新增加一筆資料由系統加減[現有寄放量]</span>");
            setCmpShowCondition_D(false, false, true, false, false, true, false, true, false, true);
        }

        f2.findField('x').setValue(x);
        f2.findField('EXTYPE').setReadOnly(false);
        f2.findField('MEMO').setReadOnly(false);
        f2.findField('STATUS').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

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
        }],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: false,
            selectable: function (record) {
                return record.data.STATUS == 'B';
            }
        }),
        columns: [{
            dataIndex: 'STATUS',
            text: "狀態代碼",
            hidden: true
        }, {
            dataIndex: 'EXTYPE',
            text: "異動類別代碼",
            hidden: true
        }, {
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "交易日期",
            dataIndex: 'TXTDAY_DISPLAY',
            width: 90,
            sortable: true
        }, {
            text: "異動類別",
            dataIndex: 'EXTYPE_NAME',
            width: 70,
            sortable: true
        }, {
            text: "異動數量",
            dataIndex: 'QTY',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0'
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 400,
            sortable: true
        }, {
            text: "狀態",
            dataIndex: 'STATUS_NAME',
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

            itemclick: function (self, record, item, index, e, eOpts) {
                //T2Rec = records.length;
                T2LastRec = record;
                setFormT2a();
                if (T2LastRec) {
                    msglabel("訊息區:");
                }
            }
        }
    });

    //點選detail項目後
    function setFormT2a() {

        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            var aa = f.findField('SEQ').getValue();

            f.findField('x').setValue('U');
            var u = f.findField('AGEN_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            if (T1Form.getForm().findField("STATUS").getValue() == "D" ||
                T2Form.getForm().findField("STATUS").getValue() == "B") {
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#confirm_return').setDisabled(true);
            }
            else {
                T2Grid.down('#edit').setDisabled(false);
                T2Grid.down('#confirm_return').setDisabled(false);
            }
        }
        else {
            T2Form.getForm().reset();
        }
    }

    //Detail按儲存之後
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition_D(false, true, false, false, true, false, true, false, false, true);
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            msglabel("訊息區: 資料新增成功");
                            r.commit();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            msglabel("訊息區: 資料修改成功");
                            r.commit();
                            break;
                        case "D":
                            T2Store.remove(r);
                            msglabel("訊息區: 資料刪除成功");
                            r.commit();
                            break;
                    }

                    T2Cleanup();
                    T2Load();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            msglabel("訊息區: Form fields may not be submitted with invalid values");
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            msglabel("訊息區: Ajax communication failed");
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            msglabel("訊息區: " + action.result.msg);
                            break;
                    }
                }
            });
        }
    }

    function Confirm_Return() {
        var selection = T2Grid.getSelection();
        let agen_no = '';
        let mmcode = '';
        let txtday = '';
        let seq = '';
        let extype = '';
        let qty = '';
        var qty_10 = 0;
        var qty_31 = 0;
        var dept = '';
        //selection.map(item => {
        //    agen_no += item.get('AGEN_NO') + ',';
        //    mmcode += item.get('MMCODE') + ',';
        //    txtday += item.get('TXTDAY_DISPLAY') + ',';
        //    seq += item.get('SEQ') + ',';
        //    extype += item.get('EXTYPE') + ',';
        //    qty += item.get('QTY') + ',';
        //    dept += item.get('DEPT') + ',';
        //    if (item.get('EXTYPE') == 10) {
        //        qty_10 = parseInt(qty_10) + parseInt(item.get('QTY'));
        //    }
        //    else if (item.get('EXTYPE') == 31) {
        //        qty_31 = parseInt(qty_31) + parseInt(item.get('QTY'));
        //    }
        //});
        $.map(selection, function (item, key) {
            agen_no += item.get('AGEN_NO') + ',';
            mmcode += item.get('MMCODE') + ',';
            txtday += item.get('TXTDAY_DISPLAY') + ',';
            seq += item.get('SEQ') + ',';
            extype += item.get('EXTYPE') + ',';
            qty += item.get('QTY') + ',';
            dept += item.get('DEPT') + ',';
            if (item.get('EXTYPE') == 10) {
                qty_10 = parseInt(qty_10) + parseInt(item.get('QTY'));
            }
            else if (item.get('EXTYPE') == 31) {
                qty_31 = parseInt(qty_31) + parseInt(item.get('QTY'));
            }
        })

        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T2Set,
                method: reqVal_p,
                params: {
                    AGEN_NO: agen_no,
                    MMCODE: mmcode,
                    TXTDAY: txtday,
                    SEQ: seq,
                    EXTYPE: extype,
                    QTY: qty,
                    DEPT: dept
                },
                //async: true,
                success: function (response) {
                    myMask.hide();
                    T2Load();

                    msglabel('訊息區: 點選的資料已確認回傳');
                    myMask.hide();
                    //viewport.down('#form').setCollapsed("true");
                    T2LastRec = false;
                    T2Form.getForm().reset();
                    T2Load();
                    //viewport.down('#t2Grid').unmask();
                    var T1_AGEN_NO = T1Form.getForm().findField("AGEN_NO").getValue();
                    var T1_MMCODE = T1Form.getForm().findField("MMCODE").getValue();
                    var T1_QTY = T1Form.getForm().findField("QTY").getValue();
                    for (var i = 0; i < T1Grid.getStore().data.items.length; i++) {
                        if ((T1Grid.getStore().data.items[i].data.AGEN_NO == T1_AGEN_NO) &&
                            (T1Grid.getStore().data.items[i].data.MMCODE == T1_MMCODE)) {

                            T1Grid.getStore().data.items[i].data.QTY = parseInt(T1_QTY) - parseInt(qty_10) + parseInt(qty_31);
                            T1Form.getForm().findField("QTY").setValue(parseInt(T1_QTY) - parseInt(qty_10) + parseInt(qty_31));
                            T1Form.getForm().getRecord().commit();
                        }

                    }
                },
                failure: function (form, action) {
                    myMask.hide();

                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            msglabel("訊息區: Form fields may not be submitted with invalid values");
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            msglabel("訊息區: Ajax communication failed");
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            msglabel("訊息區: " + action.result.msg);
                            break;
                    }
                }
            })
        }
    }

    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.readOnly = true;
            fc.setReadOnly(true);

        });
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        //viewport.down('#form').setTitle('瀏覽');
        TATabs.down('#Form').setTitle('瀏覽');
        setFormT2a();
        setCmpShowCondition_D(false, true, false, false, true, false, true, false, false, true);
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
        }]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?AGEN_NO=' + T1QueryForm.getForm().findField('P0').getValue()
                + '&MMCODE=' + T1QueryForm.getForm().findField('P1').getValue()
                + '&MMNAME_C=' + T1QueryForm.getForm().findField('P2').getValue()
                + '&MMNAME_E=' + T1QueryForm.getForm().findField('P2').getValue()
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

    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }
    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
    }

    function setCmpShowCondition_D(TXTDAY, TXTDAY_TEXT, TXTDAY_DISPLAY, EXTYPE, EXTYPE_TEXT, EXTYPE_DISPLAY, QTY, QTY_DISPLAY, STATUS, STATUS_TEXT) {
        var f = T2Form.getForm();
        TXTDAY ? showComponent(f, "TXTDAY") : hideComponent(f, "TXTDAY");
        TXTDAY_TEXT ? showComponent(f, "TXTDAY_TEXT") : hideComponent(f, "TXTDAY_TEXT");
        TXTDAY_DISPLAY ? showComponent(f, "TXTDAY_DISPLAY") : hideComponent(f, "TXTDAY_DISPLAY");
        EXTYPE ? showComponent(f, "EXTYPE") : hideComponent(f, "EXTYPE");
        EXTYPE_TEXT ? showComponent(f, "EXTYPE_TEXT") : hideComponent(f, "EXTYPE_TEXT");
        EXTYPE_DISPLAY ? showComponent(f, "EXTYPE_DISPLAY") : hideComponent(f, "EXTYPE_DISPLAY");
        QTY ? showComponent(f, "QTY") : hideComponent(f, "QTY");
        QTY_DISPLAY ? showComponent(f, "QTY_DISPLAY") : hideComponent(f, "QTY_DISPLAY");
        STATUS ? showComponent(f, "STATUS") : hideComponent(f, "STATUS");
        STATUS_TEXT ? showComponent(f, "STATUS_TEXT") : hideComponent(f, "STATUS_TEXT");
    }

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

    setCmpShowCondition_D(false, true, false, false, true, false, true, false, false, true);
    T1QueryForm.getForm().findField('P1').focus();
    SetAGEN_NO_Q();
    SetEXTYPE();
    SetSTATUS_D();
});
