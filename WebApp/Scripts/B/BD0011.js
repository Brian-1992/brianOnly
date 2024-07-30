Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
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

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.FileGridField', 'WEBAPP.form.DocButton']);
Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    //＊＊＊＊＊＊＊＊＊＊＊＊＊ 參數 ＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // var T1Get = '/api/BD0011/All'; // 查詢(改為於store定義)
        var T1Set = ''; // 新增/修改/刪除
        var GetDOCID = '../../../api/BD0011/GetDocid';
        var T1Name = "新訊息通知廠商";

        var T1Rec = 0;
        var T1LastRec = null;

        var startdate = '';
        var docid = '';
        var edit_flag = 'N';
        var agen_no_temp = '';

        var viewModel = Ext.create('WEBAPP.store.BD0011VM');

        //取得當月第一天日期
        function GetDate() {
            startdate = new Date();
            startdate.setDate(1);
            startdate = Ext.Date.format(startdate, "Ymd") - 19110000;

            T1Query.getForm().findField('P0').setValue(startdate);
            T1Query.getForm().findField('P1').setValue(new Date());
        }

        //取得識別號
        function SetDocid() { //取得調帳單號
            Ext.Ajax.request({
                url: GetDOCID,
                method: reqVal_g,
                success: function (response) {
                    T1Form.getForm().findField('DOCID').setValue(response.responseText.replace(new RegExp('"', 'g'), ''));
                    docid = response.responseText.replace(new RegExp('"', 'g'), '');
                },
                failure: function (response, options) {
                    Ext.Msg.alert('失敗', "<span style='color:red'>取得識別號失敗</span>");
                    msglabel(" <span style='color:red'>取得識別號失敗</span>");
                }
            });
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊ T1 ＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        // 查詢欄位
        var T1Query = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'datefield',
                        fieldLabel: '通知日期',
                        name: 'P0',
                        id: 'P0',
                        enforceMaxLength: true,
                        allowBlank: true, // 欄位是否為必填
                        //format: 'X/m/d',
                        labelWidth: 80,
                        width: 240,
                        renderer: function (value, meta, record) {
                            return Ext.util.Format.date(value, 'X/m/d');
                        }
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P1',
                        id: 'P1',
                        enforceMaxLength: true,
                        allowBlank: true, // 欄位是否為必填
                        //format: 'X/m/d',
                        labelWidth: 30,
                        width: 190,
                        renderer: function (value, meta, record) {
                            return Ext.util.Format.date(value, 'X/m/d');
                        }
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            T1Load();
                            msglabel("");
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            msglabel("");
                            f.reset();
                            f.findField('P0').setValue(startdate);
                            f.findField('P1').setValue(new Date());
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }]
            }]
        });

        var T1Store = viewModel.getStore('Master');

        // toolbar,包含換頁、新增/修改/刪除鈕
        var T1Tool = Ext.create('Ext.PagingToolbar', {
            store: T1Store,
            displayInfo: true,
            border: false,
            plain: true,
            buttons: [
                {
                    text: '新增', handler: function () {
                        T1Set = '/api/BD0011/Create'; // BD0011Controller的Create
                        setFormT1('I', '新增');
                    }
                },
                {
                    itemId: 'edit', text: '修改', disabled: true, handler: function () {
                        T1Set = '/api/BD0011/Update';
                        setFormT1("U", '修改');
                    }
                }
                , {
                    itemId: 'delete', text: '刪除', disabled: true,
                    handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/BD0011/Delete';
                                T1Form.getForm().findField('x').setValue('D');
                                T1Submit();
                            }
                        });
                    }
                },
                {
                    itemId: 'send', text: '寄送MAIL', disabled: true, handler: function () {
                        msglabel("");
                        Ext.MessageBox.confirm('提醒', '是否進行MAIL發送作業？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Set = '/api/BD0011/SendMail';
                                T1Form.getForm().findField('x').setValue('S');
                                T1Submit();
                            }
                        });
                    }
                },
                {
                    itemId: 'failContent', id:'failContent', text: '寄件失敗內容', disabled: true, handler: function () {
                        T3Load();
                        failContentWindow.show();
                    }
                }]
        });

        // 顯示明細/新增/修改輸入欄
        var T1Form = Ext.widget({
            xtype: 'form',
            layout: { type: 'table', columns: 1 },
            //layout: 'form',
            frame: false,
            cls: 'T1b',
            title: '',
            autoScroll: true,
            bodyPadding: '10',
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
                fieldLabel: '狀態',
                name: 'STATUS',
                xtype: 'hidden'
            }, {
                fieldLabel: '通知廠商',
                name: 'OPT_TEXT',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '識別號',
                name: 'DOCID',
                readOnly: true,
                submitValue: true
            }, {
                fieldLabel: '通知日期',
                name: 'SEND_DT',
                xtype: 'datefield',
                readOnly: true,
                allowBlank: false,
                blankText: "請選擇通知日期",
                fieldCls: 'required',
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'Xmd His');
                }
            }, {
                fieldLabel: '通知日期', //白底顯示
                name: 'SEND_DT_DISPLAY',
                readOnly: true,
                fieldCls: 'required'
            },
            {
                fieldLabel: '主旨',
                name: 'THEME',
                enforceMaxLength: true,
                maxLength: 80,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                blankText: "請輸入主旨",
                readOnly: true
            }, {
                xtype: 'textareafield',
                scrollable: true,
                fieldLabel: '訊息內容',
                name: 'MSG',
                enforceMaxLength: true,
                maxLength: 4000,
                height: 200,
                width: "100%",
                allowBlank: false, // 欄位為必填
                //fieldCls: 'required',
                readOnly: true
            }, {
                xtype: 'radiogroup',
                name: 'OPT',
                id: 'OPT',
                fieldLabel: '通知廠商',
                labelWidth: 65,
                width: 200,
                columns: 1,
                fieldCls: 'required',
                editable: false,
                allowBlank: false,
                items: [
                    {
                        boxLabel: '全部', id: 'OPT_ALL', inputValue: 'A', width: 150, checked: false
                    },
                    {
                        boxLabel: '藥品廠商', id: 'OPT_MED', inputValue: 'E', width: 150, checked: false,
                    },
                    {
                        boxLabel: '衛材廠商', id: 'OPT_MAT', inputValue: 'T', width: 150, checked: false
                    },
                    {
                        boxLabel: '一般物品廠商', id: 'OPT_GEN', inputValue: 'G', width: 150, checked: false
                    },
                    {
                        boxLabel: '其他廠商', id: 'OPT_OTHER', inputValue: 'O', width: 150, checked: false
                    },
                    {
                        boxLabel: '部份廠商', id: 'OPT_PART', inputValue: 'P', width: 150, checked: false
                    }
                ],
                listeners: {
                    change: function (field, newValue, oldValue) {
                        if (newValue.OPT == 'A' || newValue.OPT == 'G') {
                            Ext.getCmp("AGEN_NO_C").hide();
                            T1Form.getForm().findField('AGEN_NO').setValue('');
                        }
                        else if (newValue.OPT == 'P' && edit_flag == 'Y') {
                            Ext.getCmp("AGEN_NO_C").show();
                            T1Form.getForm().findField('AGEN_NO').setValue(agen_no_temp);
                        }
                    }
                }
            }, {
                xtype: 'container',
                id: 'AGEN_NO_C',
                border: false,
                layout: 'form',
                padding: '0',
                height: 30,
                items: [{
                    xtype: 'button',
                    text: '廠商清單', //白底顯示
                    name: 'AGEN_NO_C',
                    readOnly: true,
                    margin: '0 0 0 50',
                    width: 100,
                    handler: function () {
                        newApplyWindow.show();
                    }
                }]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '廠商清單', //白底顯示
                name: 'AGEN_NO',
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'filegrid',
                fieldLabel: '附加檔案',
                name: 'FILENAME',
                width: '100%',
                height: 100
            }],
            buttons: [{
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (((T1Form.getForm().findField("SEND_DT").getValue()) == null) ||
                        (T1Form.getForm().findField("SEND_DT").getValue()) == '') {
                        Ext.Msg.alert('提醒', "<span style='color:red'>通知日期不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>通知日期不可為空</span>，請重新輸入。");
                    }
                    else if (((T1Form.getForm().findField("THEME").getValue())) == null ||
                        (T1Form.getForm().findField("THEME").getValue()).trim() == '') {
                        Ext.Msg.alert('提醒', "<span style='color:red'>主旨不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>主旨不可為空</span>，請重新輸入。");
                    }
                    else if (((T1Form.getForm().findField("MSG").getValue())) == null ||
                        (T1Form.getForm().findField("MSG").getValue()).trim() == '') {
                        Ext.Msg.alert('提醒', "<span style='color:red'>訊息內容不可為空</span>，請重新輸入。");
                        msglabel(" <span style='color:red'>訊息內容不可為空</span>，請重新輸入。");
                    }
                    else if (T1Form.getForm().findField('OPT').getValue().OPT == 'O') {
                        Ext.Msg.alert('提醒', "請選擇<span style='color:red'>通知廠商</span>");
                        msglabel("請選擇<span style='color:red'>通知廠商</span>");
                    }
                    else {
                        if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            });
                        }
                        else {
                            Ext.Msg.alert('提醒', '輸入資料格式有誤');
                            msglabel(" 輸入資料格式有誤");
                        }
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
            dockedItems: [
                {
                    items: [T1Query]
                }, {
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
                text: "通知日期",
                dataIndex: 'SEND_DT_DISPLAY',
                width: 85
            }, {
                text: "主旨",
                dataIndex: 'THEME',
                width: 80
            }, {
                text: "訊息內容",
                dataIndex: 'MSG',
                width: 300
            }, {
                text: "狀態",
                dataIndex: 'STATUS_NAME',
                width: 80
            }, {
                text: "通知廠商",
                dataIndex: 'OPT_DISPLAY',
                width: 120
            }, {
                text: "廠商",
                dataIndex: 'AGEN_NO',
                width: 300
            }, {
                header: "",
                flex: 1
            }],
            listeners: {
                itemclick: function (self, record, item, index, e, eOpts) {
                    
                    T1Rec = index;
                    T1LastRec = record;
                    setFormT1a();
                    if (T1LastRec) {
                        msglabel("");
                    }

                    if (record.data.STATUS == '84') {
                        Ext.getCmp('failContent').enable();
                    } else {
                        Ext.getCmp('failContent').disable();
                    }
                }
            }
        });

        //點選master的項目後
        function setFormT1a() {
            
            if (T1LastRec) {
                T1Form.loadRecord(T1LastRec);
                if (T1Form.getForm().findField("STATUS").getValue() != '80') {
                    T1Grid.down('#send').setDisabled(true);
                    T1Grid.down('#edit').setDisabled(true);
                    T1Grid.down('#delete').setDisabled(true);
                }
                else {
                    T1Grid.down('#send').setDisabled(false);
                    T1Grid.down('#edit').setDisabled(false);
                    T1Grid.down('#delete').setDisabled(false);
                }
                isNew = false;
                viewport.down('#form').expand();

                if (T1Form.getForm().findField('OPT_TEXT').getValue() == 'A') {
                    T1Form.getForm().findField("OPT_ALL").setValue(true);
                    T1Form.getForm().findField("OPT_MED").setValue(false);
                    T1Form.getForm().findField("OPT_MAT").setValue(false);
                    T1Form.getForm().findField("OPT_GEN").setValue(false);
                    T1Form.getForm().findField("OPT_PART").setValue(false);
                } else if (T1Form.getForm().findField('OPT_TEXT').getValue() == 'G') {
                    T1Form.getForm().findField("OPT_ALL").setValue(false);
                    T1Form.getForm().findField("OPT_MED").setValue(false);
                    T1Form.getForm().findField("OPT_MAT").setValue(false);
                    T1Form.getForm().findField("OPT_GEN").setValue(true);
                    T1Form.getForm().findField("OPT_PART").setValue(false);
                } else if (T1Form.getForm().findField('OPT_TEXT').getValue() == 'T') {
                    T1Form.getForm().findField("OPT_ALL").setValue(false);
                    T1Form.getForm().findField("OPT_MED").setValue(false);
                    T1Form.getForm().findField("OPT_MAT").setValue(true);
                    T1Form.getForm().findField("OPT_GEN").setValue(false);
                    T1Form.getForm().findField("OPT_PART").setValue(false);
                } else if (T1Form.getForm().findField('OPT_TEXT').getValue() == 'E') {
                    T1Form.getForm().findField("OPT_ALL").setValue(false);
                    T1Form.getForm().findField("OPT_MED").setValue(true);
                    T1Form.getForm().findField("OPT_MAT").setValue(false);
                    T1Form.getForm().findField("OPT_GEN").setValue(false);
                    T1Form.getForm().findField("OPT_PART").setValue(false);
                }else {
                    T1Form.getForm().findField("OPT_ALL").setValue(false);
                    T1Form.getForm().findField("OPT_MED").setValue(false);
                    T1Form.getForm().findField("OPT_MAT").setValue(false);
                    T1Form.getForm().findField("OPT_GEN").setValue(false);
                    T1Form.getForm().findField("OPT_PART").setValue(true);
                }
            }
            else {
                T1Form.getForm().reset();
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#delete').setDisabled(true);
                viewport.down('#form').setCollapsed("true");
            }
        }

        //點選新增/修改按鈕時
        function setFormT1(x, t) {
            viewport.down('#t1Grid').mask();
            viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            msglabel("");
            setCmpShowCondition(true, false);
            edit_flag = 'Y';
            if (x === "I") {
                isNew = true;
                f.reset();
                var r = Ext.create('WEBAPP.model.BD0011');
                T1Form.loadRecord(r);
                f.clearInvalid();
                SetDocid();
                u = f.findField('SEND_DT');
            }
            else {
                u = f.findField('SEND_DT');
                if (f.findField('OPT').getValue().OPT == 'A' || f.findField('OPT').getValue().OPT == 'G') {
                    Ext.getCmp("AGEN_NO_C").hide();
                }
                else if (f.findField('OPT').getValue().OPT == 'P') {
                    Ext.getCmp("AGEN_NO_C").show();
                }
                agen_no_temp = f.findField('AGEN_NO').getValue();
                Ext.getCmp('OPT').readOnly = true;
            }
            f.findField('x').setValue(x);
            f.findField('MSG').setReadOnly(false);
            f.findField('OPT').setReadOnly(false);
            f.findField('SEND_DT').setReadOnly(false);
            f.findField('THEME').setReadOnly(false);
            f.findField('FILENAME').setReadOnly(false);
            T1Form.down('#cancel').setVisible(true);
            T1Form.down('#submit').setVisible(true);
            u.focus();
        }

        function T1Submit() {
            var f = T1Form.getForm();

            if (f.isValid()) {
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();
                var SEND_DT_TEMP = T1Form.getForm().findField('SEND_DT').rawValue;
                f.submit({
                    url: T1Set,
                    success: function (form, action) {
                        myMask.hide();
                        var f2 = T1Form.getForm();
                        var r = f2.getRecord();
                        switch (f2.findField("x").getValue()) {
                            case "I":
                                T1Query.getForm().findField('P0').setValue(SEND_DT_TEMP);
                                T1Query.getForm().findField('P1').setValue(SEND_DT_TEMP);
                                msglabel('訊息區:資料新增成功');
                                break;
                            case "U":
                                msglabel('訊息區:資料修改成功');
                                break;
                            case "D":
                                msglabel('訊息區:資料刪除成功');
                                break;
                            case "S":
                                msglabel('訊息區:已進行MAIL發送作業');
                                break;
                        }
                        T1Load();
                        T1Cleanup();
                    },
                    failure: function (form, action) {
                        myMask.hide();
                        switch (action.failureType) {
                            case Ext.form.action.Action.CLIENT_INVALID:
                                Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                msglabel('Form fields may not be submitted with invalid values');
                                break;
                            case Ext.form.action.Action.CONNECT_FAILURE:
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                                msglabel('Ajax communication failed');
                                break;
                            case Ext.form.action.Action.SERVER_INVALID:
                                Ext.Msg.alert('失敗', action.result.msg);
                                msglabel(action.result.msg);
                                break;
                        }
                    }
                });
            }
        }

        function T1Cleanup() {
            viewport.down('#t1Grid').unmask();
            edit_flag = 'N';
            var f = T1Form.getForm();
            f.reset();
            f.getFields().each(function (fc) {
                fc.readOnly = true;
                fc.setReadOnly(true);
            });

            T1Form.down('#cancel').hide();
            T1Form.down('#submit').hide();
            viewport.down('#form').setTitle('瀏覽');
            viewport.down('#form').setCollapsed("true");
            setCmpShowCondition(false, true);
            Ext.getCmp("AGEN_NO_C").hide();
            agen_no_temp = "";
            setFormT1a();
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 欄位設定彈出視窗 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
        var windowWidth = $(window).width();
        var windowHeight = $(window).height();
        var T2LastRec = null;

        var T2Store = viewModel.getStore('Agens');

        //var T2Store = Ext.create('WEBAPP.store.BD0011VM', { // 定義於/Scripts/app/store/BD0011VM.js
        //    listeners: {
        //        beforeload: function (store, options) {
        //            // 載入前將查詢條件P0~P4的值代入參數
        //            var np = {
        //            };
        //            Ext.apply(store.proxy.extraParams, np);
        //        }
        //    }
        //});

        //Tool
        var T2Tool = Ext.create('Ext.PagingToolbar', {
            store: T2Store,
            displayInfo: true,
            border: false,
            plain: true
        });

        //Query
        var T2Query = Ext.widget({
            xtype: 'form',
            layout: 'form',
            border: false,
            autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
            fieldDefaults: {
                xtype: 'textfield',
                labelAlign: 'right'
            },
            items: [{
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'textfield',
                    fieldLabel: '廠商代碼',
                    name: 'P0',
                    labelWidth: 80,
                    width: 250
                }, {
                    xtype: 'textfield',
                    fieldLabel: '廠商名稱',
                    name: 'P1',
                    labelWidth: 80,
                    width: 250
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        T2Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').setValue('');
                        f.findField('P1').setValue('');
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
                ]
            }]
        });

        //Grid
        var T2Grid = Ext.create('Ext.grid.Panel', {
            //title: T1Name,
            store: T2Store,
            plain: true,
            loadingText: '處理中...',
            loadMask: true,
            cls: 'T1',
            /* dockedItems: [{
                 items: [T2Query]
             }, {
                 dock: 'top',
                 xtype: 'toolbar',
                 items: [T2Tool]
                 }],*/
            selModel: Ext.create('Ext.selection.CheckboxModel', {
                checkOnly: false,
                injectCheckbox: 'first',
                mode: 'SIMPLE',
                showHeaderCheckbox: false,
                selectable: function (record) {
                    return (record.get('EMAIL') == null);
                },
            }),
            columns: [{
                xtype: 'rownumberer',
                width: 30,
                align: 'Center',
                labelAlign: 'Center',
                width: 35
            }, {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 100
            },
            {
                text: "廠商名稱",
                dataIndex: 'AGEN_NAMEC',
                width: 250
            },
            {
                text: "EMAIL",
                dataIndex: 'EMAIL',
                width: 250
            },
            {
                header: "",
                flex: 1
            }],
            viewConfig: {
                listeners: {
                    refresh: function (view) {
                        var s_string = T1Form.getForm().findField('AGEN_NO').getValue();
                        for (var i = 0; i < view.dataSource.data.length; i++) {
                            if (s_string.indexOf(view.dataSource.data.items[i].data.AGEN_NO) != -1) {
                                T2Grid.getSelectionModel().select(i, true, true);
                            }
                        }

                    }
                }
            },
            listeners: {
            }
        });

        //儲存勾選廠商
        function SelectAgen() {
            var selection = T2Grid.getSelection();
            let AGEN_NO = '';
            let AGEN_NAMEC = '';
            //selection.map(item => {
            //    AGEN_NO += item.get('AGEN_NO') + ',';
            //    AGEN_NAMEC += item.get('AGEN_NAMEC') + ',';
            //});
            $.map(selection, function (item, key) {
                AGEN_NO += item.get('AGEN_NO') + ',';
                AGEN_NAMEC += item.get('AGEN_NAMEC') + ',';
            })
            T1Form.getForm().findField('AGEN_NO').setValue(AGEN_NO.substring(0, AGEN_NO.length - 1));
            agen_no_temp = AGEN_NO.substring(0, AGEN_NO.length - 1);
        }

        //整體彈出視窗
        var newApplyWindow = Ext.create('Ext.window.Window', {
            renderTo: Ext.getBody(),
            modal: true,
            items: [
                {
                    xtype: 'panel',
                    itemId: 't2Grid',
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    border: false,
                    height: windowHeight - 63,
                    items: [T2Grid]
                }
            ],
            width: "700px",
            height: windowHeight,
            resizable: false,
            closable: false,
            y: 0,
            title: "選擇廠商",
            buttonAlign: 'center',
            buttons: [{
                text: '確定',
                handler: function () {
                    SelectAgen();
                    this.up('window').hide();
                }
            }, {
                text: '關閉',
                handler: function () {
                    this.up('window').hide();
                }
            }],
            listeners: {
                show: function (self, eOpts) {
                    newApplyWindow.setY(0);
                    T2Load();
                }
            }
        });
    }

    //#region 寄件失敗內容
    var T3Store = viewModel.getStore('ErrorLog');
    function T3Load() {
        
        if (T1LastRec != null && T1LastRec.data["DOCID"] !== '') {
            T3Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCID"]);
            T3Tool.moveFirst();
        }
    }
    var T3Tool = Ext.create('Ext.PagingToolbar', {
        store: T3Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T3Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T3Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T3Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "寄件時間",
                dataIndex: 'LOGTIME',
                width: 150
            },
            {
                text: "錯誤訊息",
                dataIndex: 'MSG',
                width: 400
            }
        ],

    });

    var failContentWindow = Ext.create('Ext.window.Window', {
        id: 'failContentWindow',
        renderTo: Ext.getBody(),
        items: [T3Grid],
        modal: true,
        width: $(window).width(),
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "寄件失敗內容",
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    failContentWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                failContentWindow.center();
                //failContentWindow.setWidth(400);
            }
        }
    });
    failContentWindow.hide();
    //#endregion

    //＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義畫面內容 ＊＊＊＊＊＊＊＊＊＊＊＊＊
    {
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
                width: 350,
                title: '瀏覽',
                border: false,
                // collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }]
        });

        function showComponent(form, fieldName) {
            var u = form.findField(fieldName);
            u.show();
        }

        function hideComponent(form, fieldName) {
            var u = form.findField(fieldName);
            u.hide();
        }

        //控制不可更改項目的顯示f
        function setCmpShowCondition(SEND_DT, SEND_DT_DISPLAY) {
            var f = T1Form.getForm();
            SEND_DT ? showComponent(f, "SEND_DT") : hideComponent(f, "SEND_DT");
            SEND_DT_DISPLAY ? showComponent(f, "SEND_DT_DISPLAY") : hideComponent(f, "SEND_DT_DISPLAY");
        }
    }
    //＊＊＊＊＊＊＊＊＊＊＊＊＊ LOAD ＊＊＊＊＊＊＊＊＊＊＊＊＊
    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').rawValue);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Tool.moveFirst();
    }
    function T2Load() {
        T2Tool.moveFirst();
    }

    viewport.down('#form').setCollapsed("true");
    T1Query.getForm().findField('P0').focus();
    GetDate();
    setCmpShowCondition(false, true);
    Ext.getCmp("AGEN_NO_C").hide();
});
