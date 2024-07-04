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

    // var T1Get = '/api/BC0005/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "小額採購訂單維護";

    var T1Rec = 0;
    var T1LastRec = null;

    var startdate = '';
    var enddate = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //取得當月第一天及最後一天日期
    function GetDate() {
        startdate = new Date();
        startdate.setDate(1);
        startdate = Ext.Date.format(startdate, "Ymd") - 19110000;

        enddate = new Date();
        enddate = Ext.Date.format(enddate, "Ymd") - 19110000;
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
            labelWidth: 90,
            width: 280
        },
        defaultType: 'textfield',
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '小額採購申請日期',
                name: 'P0',
                id: 'P0',
                enforceMaxLength: true,
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入申請日期(起)",
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d');
                }
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                allowBlank: false, // 欄位是否為必填
                fieldCls: 'required',
                blankText: "請輸入申請日期(迄)",
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d');
                }
            }],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                msglabel("");
                T1Load();
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                f.findField('P0').setValue(startdate);
                f.findField('P1').setValue(enddate);
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BC0005M', { // 定義於/Scripts/app/store/BC0005M.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').rawValue,
                    p1: T1QueryForm.getForm().findField('P1').rawValue
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
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/BC0005/UpdateM';
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'cancel_order', text: '取消訂單', disabled: true, handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('取消訂單', '是否確定取消訂單? 該小額採購申請單號將恢復[消審會核准]', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BC0005/CANCEL_ORDER';
                            T1Form.getForm().findField('x').setValue('C');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'send_mail', text: '寄送MAIL', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('寄送MAIL', '是否要進行採購訂單MAIL發送作業？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BC0005/SEND_MAIL';
                            T1Form.getForm().findField('x').setValue('S');
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
                fieldLabel: '訂單狀態',
                name: 'PO_STATUS',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '小額採購申請單號', //白底顯示
                name: 'SDN',
                submitValue: true,
                width: '100%'
            }, {
                xtype: 'displayfield',
                fieldLabel: '訂單編號',//白底顯示
                name: 'PO_NO',
                submitValue: true,
                width: '100%'
            }, {
                xtype: 'displayfield',
                fieldLabel: '廠商', //白底顯示
                name: 'AGEN_NAMEC',
                readOnly: true,
                submitValue: true,
                width: '100%'
            }, {
                xtype: 'displayfield',
                fieldLabel: '訂單狀態', //白底顯示
                name: 'PO_STATUS_CODE',
                readOnly: true,
                width: '100%'
            }, {
                xtype: 'textarea', //多行輸入框
                scrollable: true,
                fieldLabel: '備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 4000,
                height: 200,
                readOnly: true
            }, {
                xtype: 'displayfield', //白底顯示
                fieldLabel: '備註',
                name: 'MEMO_DISPLAY',
                readOnly: true,
                width: '100%'
            }, {
                xtype: 'textarea',
                scrollable: true,
                fieldLabel: 'MAIL特殊備註', //白底顯示
                name: 'SMEMO',
                enforceMaxLength: true,
                maxLength: 4000,
                height: 200,
                readOnly: true
            }, {
                xtype: 'displayfield',//多行輸入框
                fieldLabel: 'MAIL特殊備註',
                name: 'SMEMO_DISPLAY',
                readOnly: true,
                width: '100%'
            }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit_Update();
                        }
                    });
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel(" 輸入資料格式有誤");
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: false,
            selectable: function (record) {
                return record.data.PO_STATUS != '80';
            }
        }),
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            align: 'Center',
            labelAlign: 'Center'
        }, {
            text: "小額採購申請單號",
            dataIndex: 'SDN',
            width: 150
        }, {
            text: "訂單編號",
            dataIndex: 'PO_NO',
            width: 120
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NAMEC',
            width: 180
        }, {
            text: "訂單狀態",
            dataIndex: 'PO_STATUS_NAME',
            width: 80
        }, {
            text: "MAIL備註",
            dataIndex: 'MEMO',
            width: 150
        }, {
            text: "MAIL特殊備註",
            dataIndex: 'SMEMO',
            width: 150
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                if (T1Form.hidden === true) {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    TATabs.setActiveTab('Form');
                }
                T1Rec = record.length;
                T1LastRec = record;
                setFormT1a();
                if (T1LastRec) {
                    msglabel("");
                }
                msglabel('訊息區:');
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
            }
        }
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        //viewport.down('#form').setTitle(t + T1Name);
        TATabs.down('#Form').setTitle(t);
        TATabs.setActiveTab('Form');
        T1Form.setVisible(true);
        T2Form.setVisible(false);

        setCmpShowCondition(true, false, true, false);

        var f = T1Form.getForm();
        msglabel("");

        u = f.findField('MEMO');
        f.findField('x').setValue(x);
        f.findField('MEMO').setReadOnly(false);
        f.findField('SMEMO').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
    }

    function T1Submit_Update() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition(false, true, false, true);
                    T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "U":
                            msglabel(" 資料修改成功");
                            break;
                    }
                    myMask.hide();
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

    function T1Submit() {
        var selection = T1Grid.getSelection();
        let po_no = '';
        let sdn = '';
        //selection.map(item => {
        //    po_no += item.get('PO_NO') + ',';
        //    sdn += item.get('SDN') + ',';
        //});
        $.map(selection, function (item, key) {
            po_no += item.get('PO_NO') + ',';
            sdn += item.get('SDN') + ',';
        })

        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T1Set,
                method: reqVal_p,
                params: {
                    PO_NO: po_no,
                    SDN: sdn
                },
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition(false, true, false, true);
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "S":
                            msglabel(" 資料已進入待傳MAIL狀態");
                            break;
                        case "C":
                            msglabel(" 訂單已取消");
                            break;
                    }
                    myMask.hide();
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
        T1Grid.down('#edit').setDisabled(true);
        T1Grid.down('#cancel_order').setDisabled(true);
        T1Grid.down('#send_mail').setDisabled(true);

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');

            if (T1Form.getForm().findField("PO_STATUS").getValue() != '80') {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#cancel_order').setDisabled(true);
                T1Grid.down('#send_mail').setDisabled(true);
            }
            else {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#cancel_order').setDisabled(false);
                T1Grid.down('#send_mail').setDisabled(false);
            }

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
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();

        //viewport.down('#form').setTitle('瀏覽');
        TATabs.down('#Form').setTitle('瀏覽');
        setFormT1a();
        setCmpShowCondition(false, true, false, true);
    }

    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var T2Store = Ext.create('WEBAPP.store.BC0005D', { // 定義於/Scripts/app/store/BC0005D.js
        listeners: {
            //載入前將master的PCA固定處方頭關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("PO_NO").getValue()
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
            fieldLabel: '院內碼', //白底顯示
            name: 'MMCODE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名',//紅底顯示
            name: 'MMNAME_C',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名', //白底顯示
            name: 'MMNAME_E',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '廠牌', //白底顯示
            name: 'M_AGENLAB',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '單位', //白底顯示
            name: 'M_PURUN',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '單價', //白底顯示
            name: 'PO_PRICE',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '申購量', //白底顯示
            name: 'PO_QTY',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '單筆價', //白底顯示
            name: 'PO_AMT',
            readOnly: true
        }, {
            xtype: 'displayfield',
            fieldLabel: '備註', //白底顯示
            name: 'MEMO',
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
            width: 250,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 250,
            sortable: true
        }, {
            text: "廠牌",
            dataIndex: 'M_AGENLAB',
            width: 60,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'M_PURUN',
            width: 50,
            sortable: true
        }, {
            text: "單價",
            dataIndex: 'PO_PRICE',
            width: 85,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "申購量",
            dataIndex: 'PO_QTY',
            width: 85,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "單筆價",
            dataIndex: 'PO_AMT',
            width: 90,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 120,
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
            width: 330,
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

    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }
    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
    }

    function setCmpShowCondition(MEMO, MEMO_DISPLAY, SMEMO, SMEMO_DISPLAY) {
        var f = T1Form.getForm();
        MEMO ? showComponent(f, "MEMO") : hideComponent(f, "MEMO");
        MEMO_DISPLAY ? showComponent(f, "MEMO_DISPLAY") : hideComponent(f, "MEMO_DISPLAY");
        SMEMO ? showComponent(f, "SMEMO") : hideComponent(f, "SMEMO");
        SMEMO_DISPLAY ? showComponent(f, "SMEMO_DISPLAY") : hideComponent(f, "SMEMO_DISPLAY");
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

    T1QueryForm.getForm().findField('P0').focus();
    setCmpShowCondition(false, true, false, true);
    GetDate();
    T1QueryForm.getForm().findField('P0').setValue(startdate);
    T1QueryForm.getForm().findField('P1').setValue(enddate);
});
