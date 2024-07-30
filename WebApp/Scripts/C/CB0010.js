Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "";

    var T1Rec = 0;
    var T1LastRec = null;

    var xcategoryComboGet = '../../../api/CB0010/GetXcategoryCombo';
    var reportUrl = '/Report/C/CB0010.aspx';

    //條碼分類代碼清單
    var xcategoryQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'P0',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        allowBlank: false,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/CB0010/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {

            }
        }
    });
    var wh_NoCombo2 = Ext.create('WEBAPP.form.WH_NoCombo', {
        name: 'WH_NO',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        readOnly: true,
        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/CB0010/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {

        },
        listeners: {
            select: function (c, r, i, e) {

            }
        }
    });
    var WH_NO = "";
    var STORE_LOC = "";
    var FLAG = "";

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });


    function setComboData() {
        Ext.Ajax.request({
            url: xcategoryComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var xcategory = data.etts;
                    if (xcategory.length > 0) {
                        xcategoryQueryStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < xcategory.length; i++) {
                            xcategoryQueryStore.add({ VALUE: xcategory[i].VALUE, TEXT: xcategory[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 170;
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
            items: [wh_NoCombo,
                {
                    xtype: 'textfield',
                    fieldLabel: '儲位代碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'combobox',
                    fieldLabel: '電子儲位',
                    name: 'P2',
                    id: 'P2',
                    queryMode: 'local',
                    displayField: 'name',
                    valueField: 'abbr',
                    width: 145,
                    editable: false,
                    store: [
                        { abbr: '', name: '全部' },
                        { abbr: 'Y', name: '電子儲位' },
                        { abbr: 'N', name: '非電子儲位' }
                    ],
                    value: '',
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid()) {
                            T1Load();
                        }
                        else {
                            Ext.MessageBox.alert('提示', '請輸入必填欄位');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BC_STLOC', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    //p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });
        var f = T1Form.getForm();
        f.findField('BARCODE').allowBlank = true;
        f.findField('XCATEGORY').allowBlank = true;
        //f.findField('STATUS').allowBlank = true;
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    T1Set = '/api/CB0010/Create';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/CB0010/Update';
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'btnDelete', text: '刪除', disabled: true, handler: function () {
                    var selection = T1Grid.getSelection();
                    var wh_no = '';
                    var store_loc = '';
                    if (selection.length) {
                        wh_no = selection[0].get('WH_NO');
                        store_loc = selection[0].get('STORE_LOC');

                        Ext.MessageBox.confirm('刪除', '是否確定刪除?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/CB0010/Delete',
                                    method: reqVal_p,
                                    params: {
                                        WH_NO: wh_no,
                                        STORE_LOC: store_loc
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:資料刪除成功');
                                            T1Load();
                                            Ext.getCmp('eastform').collapse();
                                        }
                                        else {
                                            Ext.MessageBox.alert('提示', data.msg);
                                        }
                                    },
                                    failure: function (form, action) {
                                        switch (action.failureType) {
                                            case Ext.form.action.Action.CLIENT_INVALID:
                                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                break;
                                            case Ext.form.action.Action.CONNECT_FAILURE:
                                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                                break;
                                            case Ext.form.action.Action.SERVER_INVALID:
                                                Ext.Msg.alert('失敗', action.result.msg);
                                                break;
                                        }
                                    }
                                });
                            }
                        });
                    }

                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'WH_NO', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                            p.push({ name: 'STORE_LOC', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                            p.push({ name: 'FLAG', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            PostForm('/api/CB0010/Excel', p);
                        }
                    });
                }
            }, {
                itemId: 't1print', text: '列印', disabled: false, handler: function () {
                    showReport();
                }
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField('x').setValue(x);

        f.getFields().each(function (fc) {
            fc.setReadOnly(false);
        });
        if (x === "I") {
            var r = Ext.create('WEBAPP.model.BC_STLOC'); // /Scripts/app/model/BC_STLOC.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            f.findField('FLAG').setValue('Y');
        }
        if (x === "U")
        {
            f.findField('WH_NO_D').show();
            f.findField('WH_NO').hide();
            f.findField('STORE_LOC').hide();
            f.findField('STORE_LOC_D').show();

            f.findField('WH_NO_D').setValue(f.findField('WH_NO').value);
            f.findField('STORE_LOC_D').setValue(f.findField('STORE_LOC').value);
        }
        f.findField('WH_NO').allowBlank = false;
        f.findField('STORE_LOC').allowBlank = false;
        f.findField('BARCODE').allowBlank = false;


        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 80
        }, {
            text: "儲位代碼",
            dataIndex: 'STORE_LOC',
            width: 130
        }, {
            text: "儲位條碼",
            dataIndex: 'BARCODE',
            width: 130,
        }, {
            text: "條碼分類代碼",
            dataIndex: 'XCATEGORY',
            width: 150
        }, {

            text: "電子儲位",
            dataIndex: 'FLAG',
            width: 100,
            renderer: function (value) {
                if (value == 'Y')
                { return '電子儲位'; }
                if (value == 'N')
                { return '非電子儲位'; }
            }
        }, {
            text: "備註",
            dataIndex: 'MEMO',
            width: 150
        },
        {
            header: "",
            flex: 1
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Grid.down('#export').setDisabled(true);
                    T1Grid.down('#t1print').setDisabled(true);
                    var allRecords = T1Store.data;
                    //有設定儲位條碼則可列印
                    allRecords.each(function (record) {
                        if (record.data.BARCODE.toString() != '') {
                            T1Grid.down('#export').setDisabled(false);
                            T1Grid.down('#t1print').setDisabled(false);
                            return false;
                        }
                    });

                    WH_NO = T1Query.getForm().findField('P0').getValue();
                    STORE_LOC = T1Query.getForm().findField('P1').getValue();
                    FLAG = T1Query.getForm().findField('P2').getValue();

                    WH_NO == '' ? WH_NO = 'null' : "";
                    STORE_LOC == '' ? STORE_LOC = 'null' : "";
                    FLAG == '' ? FLAG = 'null' : "";

                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                viewport.down('#form').expand();
                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#btnDelete').setDisabled(T1Rec === 0);

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
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
        },
            wh_NoCombo2,
        {
            xtype: 'displayfield',
            fieldLabel: '儲位條碼',
            name: 'WH_NO_D',
            hidden: true
        },
        {
            fieldLabel: '儲位代碼',
            name: 'STORE_LOC',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true,
            fieldCls: 'required'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '儲位條碼',
            name: 'STORE_LOC_D',
            hidden: true
        }, {
            fieldLabel: '儲位條碼',
            id: 'BARCODE',
            name: 'BARCODE',
            enforceMaxLength: true,
            maxLength: 200,
            readOnly: true,
            fieldCls: 'required'

        }, {
            xtype: 'combo',
            store: xcategoryQueryStore,
            fieldLabel: '條碼分類代碼',
            id: 'XCATEGORY',
            name: 'XCATEGORY',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            autoSelect: true,
            multiSelect: false,
            editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇條碼分類代碼",
            readOnly: true,
            listeners: {
                change: function (_this, newValue, oldValue, eOpts) {
                    switch (newValue) {
                        case 'CODE39':
                            T1Form.down('#BARCODE').maxLength = 21;
                            break;
                        case 'CODE93':
                            T1Form.down('#BARCODE').maxLength = 28;
                            break;
                        case 'CODE128':
                            T1Form.down('#BARCODE').maxLength = 48;
                            break;
                        default:
                            T1Form.down('#BARCODE').maxLength = 200;
                            break;

                    }
                }

            }
        }, {
            xtype: 'combo',
            fieldLabel: '電子儲位',
            name: 'FLAG',
            queryMode: 'local',
            displayField: 'name',
            valueField: 'abbr',
            autoSelect: true,
            multiSelect: false,
            editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇電子儲位",
            readOnly: true,
            value: 'Y',
            store: [
                { abbr: 'Y', name: '電子儲位' },
                { abbr: 'N', name: '非電子儲位' }
            ]
        }, {
            fieldLabel: '備註',
            id: 'MEMO',
            name: 'MEMO',
            enforceMaxLength: true,
            maxLength: 200,
            readOnly: true,
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
                    var str = '';
                    if (T1Form.down('#XCATEGORY').value != null && T1Form.down('#XCATEGORY').value != '') {
                        str = '</br>' + T1Form.down('#XCATEGORY').value + '儲位條碼可輸入長度為' + T1Form.down('#BARCODE').maxLength;
                    }
                    Ext.Msg.alert('提醒', '輸入資料格式有誤' + str);
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        //if (f.findField("x").getValue() == 'U' && T1Grid.getSelection().length > 0)
        //{
        //    if (T1Grid.getSelection()[0].get('WH_NO') == f.findField("WH_NO").getValue() && T1Grid.getSelection()[0].get('STORE_LOC') == f.findField("STORE_LOC").getValue()) {
        //        return;
        //    }
        //}

        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                params: {
                    OLD_WH_NO: T1Grid.getSelection().length > 0 ? T1Grid.getSelection()[0].get('WH_NO') : null,
                    OLD_STORE_LOC: T1Grid.getSelection().length > 0 ? T1Grid.getSelection()[0].get('STORE_LOC') : null
                },
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.WH_NO);
                            T1Query.getForm().findField('P1').setValue(v.STORE_LOC);
                            T1Query.getForm().findField('P2').setValue('');
                            T1Load();
                            break;
                        case "U":
                            r.data.WH_NO = f2.findField('WH_NO').getValue();
                            r.data.STORE_LOC = f2.findField('STORE_LOC').getValue();
                            r.data.BARCODE = f2.findField('BARCODE').getValue();
                            r.data.XCATEGORY = f2.findField('XCATEGORY').getValue();
                            r.data.FLAG = f2.findField('FLAG').getValue();
                            r.data.MEMO = f2.findField('MEMO').getValue();
                            r.commit();
                            break;
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
        }
    }
    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        f.findField('WH_NO').show();
        f.findField('STORE_LOC').show();
        f.findField('WH_NO_D').hide();
        f.findField('STORE_LOC_D').hide();


        f.findField('WH_NO').allowBlank = true;
        f.findField('STORE_LOC').allowBlank = true;
        f.findField('BARCODE').allowBlank = true;

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT1a();
    }
    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?WH_NO=' + WH_NO + '&STORE_LOC=' + STORE_LOC + '&FLAG=' + FLAG + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);

        }
        win.show();
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
        }
            ,
        {
            itemId: 'form',
            id: 'eastform',
            region: 'east',
            collapsible: true,
            collapsed: true,
            floatable: true,
            width: 300,
            title: '瀏覽',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    T1Query.getForm().findField('P0').focus();
});
