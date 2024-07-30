Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app',
    }
});
Ext.require(['WEBAPP.utils.Common']);

var DataTime = '';

var T1GetExcel = '/api/BH0004/Excel';
Ext.onReady(function () {
    // var T1Get = '/api/BE0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "氣體廠商鋼瓶資料維護網頁";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

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


    var GetDeptCombo = '../../../api/BH0004/GetDeptCombo';

    //放置地點清單
    var DeptQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: GetDeptCombo,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var dept = data.etts;
                    if (dept.length > 0) {
                        DeptQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < dept.length; i++) {
                            DeptQueryStore.add({ VALUE: dept[i].VALUE, TEXT: dept[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();


    var IsSend = false;

    var AgenNamec = session['UserName'];



    //搜尋院內碼
    var mmCodeComboT1 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        allowBlank: false,
        fieldCls: 'required',
        readOnly: true,

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BH0004/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

            }
        }
    });
    var mmCodeComboT2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        allowBlank: false,
        fieldCls: 'required',
        readOnly: true,

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BH0004/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

            }
        }
    });
    var mmCodeCombo2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        allowBlank: true,

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BH0004/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

            }
        }
    });
    var mmCodeCombo3 = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        allowBlank: true,

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/BH0004/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值

            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 200;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    name: 'P0',
                    padding: '0 4 0 4',
                    value: '廠商碼:' + session['UserId'] + ' ' + AgenNamec
                }, {
                    xtype: 'textfield',
                    fieldLabel: '品名',
                    name: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }
                , {
                    xtype: 'textfield',
                    fieldLabel: '瓶號',
                    name: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '更換日期',
                    name: 'P3',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 60,
                    width: 145,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 15,
                    width: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'hidden',
                    name: 'P5',
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        IsSend = false;
                        T1Grid.columns[1].setVisible(false);
                        T1Grid.columns[2].setVisible(false);
                        Ext.getCmp('import').setDisabled(true);
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();



                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P5').setValue(null);
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                },
                {
                    xtype: 'filefield',
                    name: 'send',
                    buttonOnly: true,
                    buttonText: '選擇檔案匯入',
                    width: 90,
                    listeners: {
                        change: function (widget, value, eOpts) {
                            Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                            Ext.getCmp('import').setDisabled(true);
                            T1Store.removeAll();
                            T1Grid.columns[1].setVisible(false);
                            T1Grid.columns[2].setVisible(false);
                            var files = event.target.files
                            var self = this; // the controller
                            if (!files || files.length == 0) return; // make sure we got something
                            var f = files[0];
                            var ext = this.value.split('.').pop();
                            if (!/^(xls|xlsx)$/.test(ext)) {
                                Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                                T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                msglabel("訊息區:");
                            }
                            else {
                                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                                myMask.show();
                                var formData = new FormData();
                                formData.append("file", f);
                                var ajaxRequest = $.ajax({
                                    type: "POST",
                                    url: "/api/BH0004/SendExcel",
                                    data: formData,
                                    processData: false,
                                    //必須false才會自動加上正確的Content-Type
                                    contentType: false,
                                    success: function (data, textStatus, jqXHR) {
                                        if (!data.success) {
                                            T1Store.removeAll();
                                            Ext.MessageBox.alert("提示", data.msg);
                                            msglabel("訊息區:");
                                            Ext.getCmp('import').setDisabled(true);
                                            IsSend = false;
                                        }
                                        else {
                                            msglabel("訊息區:檔案讀取成功");
                                            T1Store.loadData(data.etts, false);
                                            IsSend = true;
                                            Ext.getCmp('import').setDisabled(false);
                                            T1Grid.columns[1].setVisible(true);
                                            T1Grid.columns[2].setVisible(true);
                                        }
                                        T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                        myMask.hide();

                                    },
                                    error: function (jqXHR, textStatus, errorThrown) {
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        T1Query.getForm().findField('send').fileInputEl.dom.value = '';
                                        Ext.getCmp('import').setDisabled(true);
                                        myMask.hide();

                                    }
                                });
                            }
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '匯入',
                    id: 'import',
                    name: 'import',
                    disabled: true,
                    handler: function () {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        Ext.Ajax.request({
                            url: '/api/BH0004/Import',
                            method: reqVal_p,
                            params: {
                                data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    T1Store.loadData(data.etts, false);
                                    Ext.MessageBox.alert("提示", "匯入完成");
                                    msglabel("訊息區:匯入完成");
                                    Ext.getCmp('import').setDisabled(true);
                                }
                                myMask.hide();
                            },
                            failure: function (form, action) {
                                myMask.hide();
                                switch (action.failureType) {
                                    case Ext.form.action.Action.CLIENT_INVALID:
                                        Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                        break;
                                    case Ext.form.action.Action.CONNECT_FAILURE:
                                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        break;
                                    case Ext.form.action.Action.SERVER_INVALID:
                                        Ext.Msg.alert('失敗', "匯入失敗");
                                        break;
                                }
                            }
                        });
                    }
                },
                {
                    xtype: 'button',
                    text: '下載範本',
                    handler: function () {
                        ////PostForm(T1GetExcel, null);
                        location.href = "../../Scripts/B/廠商氣體鋼瓶上傳範本檔.xls";
                        msglabel('訊息區:下載範本完成');
                    }
                },
                {
                    xtype: 'button',
                    text: '確認回傳',
                    disabled: true,
                    itemId: 'btnConfirm',
                    handler: function () {

                        Ext.MessageBox.confirm('提示', '確定將資料回傳三總？', function (btn, text) {
                            if (btn === 'yes') {
                                T1Grid.columns[1].setVisible(true);
                                Ext.Ajax.request({
                                    url: '/api/BH0004/confirmData',
                                    method: reqVal_p,
                                    params: {
                                        data: Ext.encode(Ext.pluck(T1Grid.getSelection(), 'data'))
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Store.loadData(data.etts, false);
                                            Ext.MessageBox.alert("提示", "資料回傳完成");
                                            msglabel('資料回傳完成');
                                        }
                                    },
                                    failure: function (form, action) {
                                        switch (action.failureType) {
                                            case Ext.form.action.Action.CLIENT_INVALID:
                                                Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                                break;
                                            case Ext.form.action.Action.CONNECT_FAILURE:
                                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                                                break;
                                            case Ext.form.action.Action.SERVER_INVALID:
                                                Ext.Msg.alert('失敗', action.result.msg);
                                                break;
                                        }
                                    }
                                })
                            }
                        })


                    }
                }
            ]
        }]
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'displayfield',
                    name: 'P0',
                    padding: '0 4 0 4',
                    value: '廠商碼:' + session['UserId'] + ' ' + AgenNamec
                }, {
                    xtype: 'textfield',
                    fieldLabel: '品名',
                    name: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }
                , {
                    xtype: 'textfield',
                    fieldLabel: '瓶號',
                    name: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }
            ]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '更換日期',
                    name: 'P3',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 60,
                    width: 145,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P4',
                    enforceMaxLength: true,
                    maxLength: 7,
                    labelWidth: 15,
                    width: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'hidden',
                    name: 'P5',
                }, {
                    xtype: 'button',
                    text: '查詢',

                    handler: function () {

                        Ext.ComponentQuery.query('panel[itemId=form]')[1].collapse();
                        T2Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P5').setValue(null);
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T2Query.getForm().findField('label1').setValue('<span style=color:red>資料日期:' + Ext.util.Format.date(DataTime, 'XmdHi').toString() + '(說明:異動資料非即時轉入三總，所以可能非最新資料)</span>');

                        msglabel('訊息區:');
                    }
                }
            ]
        },
        {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            items: [{
                id: 'label1',
                name: 'label1',
                xtype: 'displayfield',
                padding: '0 4 0 4',
                width: 450
            }]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.WB_AIRST', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: 'T1'

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
    }

    var T2Store = Ext.create('WEBAPP.store.WB_AIRST', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p1: T2Query.getForm().findField('P1').getValue(),
                    p2: T2Query.getForm().findField('P2').getValue(),
                    p3: T2Query.getForm().findField('P3').getValue(),
                    p4: T2Query.getForm().findField('P4').getValue(),
                    p5: T2Query.getForm().findField('P5').getValue(),
                    p6: 'T2'

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        T2Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BH0004/Create'; // BE0002Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BH0004/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/BH0004/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }
        ]
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [

        ]
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.WB_AIRHIS');
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容

            f.getFields().each(function (fc) {
                fc.setReadOnly(false);
                fc.clearInvalid();
            });
        }
        //修改
        else {
            f.getFields().each(function (fc) {
                fc.setReadOnly(false);
            });

            f.findField('TXTDAY').hide();
            f.findField('FBNO').hide();
            f.findField('TXTDAY_D').show();
            f.findField('FBNO_D').show();
        }
        f.findField('x').setValue(x);

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        //u.focus();
    }

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
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selectable: function (record) {
                return record.get('STATUS') != 'A';
            }
        },
        selType: 'checkboxmodel',
        columns: [{
            //text: "項次",
            xtype: 'rownumberer'
        }, {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 150
        }, {
            text: "異動結果",
            dataIndex: 'IMPORT_RESULT',
            width: 120
        }, {
            xtype: 'datecolumn',
            text: "更換日期",
            dataIndex: 'TXTDAY',
            width: 80,
            format: 'Xmd'
        }, {
            text: "更換類別",
            dataIndex: 'EXTYPE',
            width: 60,
            renderer: function (value) {
                var str = '';
                switch (value) {
                    case 'GO':
                        str = '取走';
                        break;
                    case 'GI':
                        str = '換入';
                        break;
                    case 'CH':
                        str = '修改';
                        break;
                    default: str = '';
                        break;
                }
                return str;
            }
        }, {
            text: "品名",
            dataIndex: 'NAMEC',
            width: 250
        }, {
            text: "瓶號",
            dataIndex: 'FBNO',
            width: 100
        }, {
            text: "容量",
            dataIndex: 'XSIZE',
            width: 70
        }, {
            text: "狀態",
            dataIndex: 'STATUS',
            width: 80,
            renderer: function (value) {
                var str = '';
                switch (value) {
                    case 'A':
                        str = '處理中';
                        break;
                    case 'B':
                        str = '確認回傳';
                        break;
                    default: str = '';
                        break;
                }
                return str;
            }
        }, {
            dataIndex: 'SEQ',
            xtype: 'hidden'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                if (!IsSend) {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                    setFormT1a();
                }
            }
        }
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }],
        columns: [{
            //text: "項次",
            xtype: 'rownumberer',
        }, {
            xtype: 'datecolumn',
            text: "更換日期",
            dataIndex: 'TXTDAY',
            width: 80,
            format: 'Xmd'
        }, {
            text: "廠商碼",
            dataIndex: 'AGEN_NO',
            width: 130
        }, {
            text: "品名",
            dataIndex: 'NAMEC',
            width: 130
        }, {
            text: "瓶號",
            dataIndex: 'FBNO',
            width: 100
        }, {
            text: "容量",
            dataIndex: 'XSIZE',
            width: 70
        }, {
            dataIndex: 'SEQ',
            xtype: 'hidden'
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[1].expand();
                setFormT2a();
            }
        }
    });


    function setFormT1a() {
        var selectCount = T1Grid.getSelection().length;
        if (selectCount == 1) {
            T1Grid.down('#edit').setDisabled(T1Rec === 0);
            T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        }
        else {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#delete').setDisabled(true); // 若有刪除鈕,可在此控制是否可以按
        }
        T1Query.down('#btnConfirm').setDisabled(!selectCount > 0);

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');

            f.findField('TXTDAY_D').setValue(T1LastRec.data['TXTDAY']);
            f.findField('FBNO_D').setValue(T1LastRec.data['FBNO']);

            if (T1LastRec.data['STATUS'] == 'B') {
                T1Grid.down('#edit').setDisabled(true); // 已轉檔的資料就不允許修改刪除
                T1Grid.down('#delete').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
        }
    }

    function setFormT2a() {
        if (T1LastRec) {
            isNew = false;
            T2Form.loadRecord(T1LastRec);
            var f = T2Form.getForm();
            f.findField('x').setValue('U');
        }
        else {
            T1Form.getForm().reset();
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
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
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            xtype: 'datefield',
            fieldLabel: '更換日期',
            name: 'TXTDAY',
            enforceMaxLength: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '更換日期',
            name: 'TXTDAY_D',
            hidden: true,
            renderer: function (value) {
                return Ext.util.Format.date(value, 'Xmd');
            }
        }, {
            xtype: 'combo',
            fieldLabel: '更換類別',
            name: 'EXTYPE',
            enforceMaxLength: true,
            queryMode: 'local',
            displayField: 'name',
            valueField: 'abbr',
            store: [
                { abbr: 'GO', name: '取走' },
                { abbr: 'GI', name: '換入' }
            ],
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '品名',
            name: 'NAMEC',
            enforceMaxLength: true,
            maxLength: 200,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            fieldLabel: '瓶號',
            name: 'FBNO',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '瓶號',
            name: 'FBNO_D',
            hidden: true
        }, {
            fieldLabel: '容量',
            name: 'XSIZE',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '狀態',
            name: 'STATUS',
            enforceMaxLength: true,
            readOnly: true,
            renderer: function (value) {
                var str = '';
                switch (value) {
                    case 'A':
                        str = '處理中';
                        break;
                    case 'B':
                        str = '確認回傳';
                        break;
                    default: str = '';
                        break;
                }
                return str;
            }
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    var T2Form = Ext.widget({
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
            labelWidth: 90
        },
        defaultType: 'displayfield',
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        }, {
            fieldLabel: '更換日期',
            name: 'TXTDAY',
            readOnly: true,
            renderer: function (value) {
                return Ext.util.Format.date(value, 'Xmd');
            }
        }, {
            fieldLabel: '廠商碼',
            name: 'AGEN_NO'
        }, {
            fieldLabel: '品名',
            name: 'NAMEC'
        },
        {
            fieldLabel: '瓶號',
            name: 'FBNO',
            enforceMaxLength: true,
            readOnly: true
        }, {
            fieldLabel: '容量',
            name: 'XSIZE',
            enforceMaxLength: true,
            readOnly: true
        }, {
            name: 'SEQ',
            xtype: 'hidden'
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );

                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            switch (f.findField("x").getValue()) {
                case "I":
                    f.submit({
                        url: T1Set,
                        success: function (form, action) {
                            myMask.hide();
                            var f2 = T1Form.getForm();
                            var r = f2.getRecord();
                            switch (f2.findField("x").getValue()) {
                                case "I":
                                    // 新增後,將key代入查詢條件,只顯示剛新增的資料
                                    var v = action.result.etts[0];
                                    T1Query.getForm().findField('P1').setValue(v.MMCODE);
                                    T1Query.getForm().findField('P2').setValue(v.FBNO);
                                    T1Query.getForm().findField('P3').setValue(new Date(v.TXTDAY));
                                    T1Query.getForm().findField('P4').setValue(new Date(v.TXTDAY));
                                    T1Query.getForm().findField('P5').setValue(v.SEQ);

                                    T1Load();
                                    msglabel('訊息區:資料新增成功');
                                    //T1Query.getForm().findField('P5').setValue(null);
                                    break;
                                //case "D":
                                //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                                //    r.commit();
                                //    break;
                            }
                            T1Cleanup();
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', action.result.msg);
                                    break;
                            }
                        }
                    });
                    break;
                case "U":

                    f.submit({
                        url: T1Set,
                        method: reqVal_p,
                        params: {
                            MMCODE_p: T1Query.getForm().findField('P1').getValue(),
                            FBNO_p: T1Query.getForm().findField('P2').getValue(),
                            TXTDAY_B_p: T1Query.getForm().findField('P3').getValue(),
                            TXTDAY_E_p: T1Query.getForm().findField('P4').getValue(),
                            FBNO_old: T1Grid.getSelection()[0].get('FBNO'),
                            MMCODE_old: T1Grid.getSelection()[0].get('MMCODE'),
                            TXTDAY_old: T1Grid.getSelection()[0].get('TXTDAY')
                        },
                        success: function (form, action) {
                            myMask.hide();
                            var f2 = T1Form.getForm();
                            var r = f2.getRecord();

                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');

                            T1Cleanup();
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', action.result.msg);
                                    break;
                            }
                        }
                    });
                    break;
                case "D":
                    f.submit({
                        url: T1Set,
                        success: function (form, action) {
                            myMask.hide();
                            var f2 = T1Form.getForm();
                            var r = f2.getRecord();
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            msglabel('訊息區:資料刪除成功');

                            T1Cleanup();
                        },
                        failure: function (form, action) {
                            myMask.hide();
                            switch (action.failureType) {
                                case Ext.form.action.Action.CLIENT_INVALID:
                                    Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                                    break;
                                case Ext.form.action.Action.CONNECT_FAILURE:
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    break;
                                case Ext.form.action.Action.SERVER_INVALID:
                                    Ext.Msg.alert('失敗', action.result.msg);
                                    break;
                            }
                        }
                    });
                    break;
            }
        }
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        f.findField('TXTDAY_D').hide();
        f.findField('FBNO_D').hide();
        f.findField('TXTDAY').show();
        f.findField('FBNO').show();

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
        Ext.getCmp('import').setDisabled(true);
    }



    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 定義TAB內容 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    var TATabs = Ext.widget('tabpanel', {
        listeners: {
            tabchange: function (tabpanel, newCard, oldCard) {
                switch (newCard.title) {
                    case "氣體鋼瓶歷史資料":
                        break;
                    case "氣體鋼瓶現況資料":
                        break;
                }
                T11Rec = 0;
                T11LastRec = null;
            }
        },
        layout: 'fit',
        plain: true,
        border: false,
        resizeTabs: true,       //改變tab尺寸       
        enableTabScroll: true,  //是否允許Tab溢出時可以滾動
        defaults: {
            // autoScroll: true,
            closabel: false,    //tab是否可關閉
            padding: 0,
            split: true
        },
        items: [{
            title: '氣體鋼瓶歷史資料',
            layout: 'border',
            padding: 0,
            split: true,
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
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form]
            }]
        },
        {
            title: '氣體鋼瓶現況資料',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                itemId: 't2Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                items: [T2Grid]
            },
            {
                itemId: 'form',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T2Form]
            }
            ]
        }
        ]
    });


    //＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊ 網頁進入點 ＊＊＊＊＊＊＊＊＊＊＊＊＊＊＊
    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'fit',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [TATabs]
    });

    T1Query.getForm().findField('P3').setValue(new Date());
    T1Query.getForm().findField('P4').setValue(new Date());

    T1Grid.columns[1].setVisible(false);
    T1Grid.columns[2].setVisible(false);

    T2Query.getForm().findField('P3').setValue(new Date());
    T2Query.getForm().findField('P4').setValue(new Date());


    function setDataTime() {
        Ext.Ajax.request({
            url: '../../../api/BH0004/GetDataTime',
            method: reqVal_g,
            success: function (response) {
                //DataTime = new Date(response.responseText.substr(1, response.responseText.length - 2));
                DataTime = Ext.decode(response.responseText);
                debugger
                T2Query.getForm().findField('label1').setValue('<span style=color:red>資料日期:' + Ext.util.Format.date(DataTime, 'XmdHi').toString() + '(說明:異動資料非即時轉入三總，所以可能非最新資料)</span>');
            },
            failure: function (response, options) {

            }
        });
    }
    setDataTime();
});
