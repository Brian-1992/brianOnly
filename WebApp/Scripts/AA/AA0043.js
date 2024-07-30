Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.DocButton']);

Ext.onReady(function () {
    // var T1Get = '/api/AA0043/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "庫房儲位檔";
    var WhnoComboGet = '../../../api/AA0043/GetWhnoCombo';
    var Store_locComboGet = '../../../api/AA0043/GetSTORE_LOC';
    var GetExcel = '/api/AA0043/Excel';
    var ChkMmcode = '/api/AA0043/ChkMmcode';

    var T1Rec = 0;
    var T1LastRec = null;
    var IsSend = false;

    var vTaskid = '0';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    Ext.define('MyApp.view.component.Number', {
        override: 'Ext.form.field.Number',
        forcePrecision: false,

        // changing behavior of valueToRaw with forcePrecision
        valueToRaw: function (value) {
            var me = this,
                decimalSeparator = me.decimalSeparator;

            value = me.parseValue(value); // parses value received from the field
            value = me.fixPrecision(value); // applies precision
            value = Ext.isNumber(value) ? value :
                parseFloat(String(value).replace(decimalSeparator, '.'));
            value = isNaN(value) ? '' : String(value).replace('.', decimalSeparator);

            // forcePrecision: true - retains a precision
            // forcePrecision: false - does not retain precision for whole numbers
            if (value == "") {
                return "";

            }
            else {
                return me.forcePrecision ? Ext.Number.toFixed(
                    parseFloat(value),
                    me.decimalPrecision)
                    :
                    parseFloat(
                        Ext.Number.toFixed(
                            parseFloat(value),
                            me.decimalPrecision
                        )
                    );
            }
        }
    });

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1']
    });
    

    //院內碼清單
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //儲位碼清單
    var Store_locStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //建立庫房代碼下拉式選單
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        //whnoQueryStore.add({ VALUE: '', TEXT: '', EXTRA1: '' });
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT, EXTRA1: wh_nos[i].EXTRA1 });
                        }
                        T1Query.getForm().findField('P0').setValue(wh_nos[0].VALUE);
                        vTaskid = wh_nos[0].EXTRA1;
                        MATQueryStore.load();
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    
    //建立儲位代碼下拉式選單
    function setStore_loc(wh_no, s) {
        Ext.Ajax.request({
            url: Store_locComboGet,
            method: reqVal_p,
            params: {
                WH_NO: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Store_locStore.removeAll();
                    var Store_locs = data.etts;
                    if (Store_locs.length > 0) {
                        for (var i = 0; i < Store_locs.length; i++) {
                            Store_locStore.add({ VALUE: Store_locs[i].VALUE, TEXT: Store_locs[i].TEXT });
                        }
                        if (s == 'U') {
                            T1Form.getForm().findField('STORE_LOC').setValue(T1Form.getForm().findField('STORE_LOC_DISPLAY').getValue());
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    //搜尋院內碼
    var mmCodeCombo_Q = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        id: 'P1',
        fieldLabel: '院內碼',
        allowBlank: true,
        width: 180,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0043/GetMMCodeCombo_Q',
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P3').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T1Query.getForm();
                if (r.get('P1') !== '') {
                    f.findField("P1").setValue(r.get('MMCODE'));
                }
            }
        }
    });

    //新增院內碼
    var mmCodeCombo_I = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        //labelWidth: 80,
        labelAlign: 'right',
        //width: 220,
        blankText: "請輸入院內碼",

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0043/GetMMCodeCombo_I',
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('WH_NO').getValue()
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                }
            },
            blur: function (field, event, eOpts) {
                // 離開此欄位時,檢查填入的院內碼是否有在基本檔
                if (field.getValue() == '' || field.getValue() == null)
                {
                    var f = T1Form.getForm();
                    f.findField("MMNAME_C").setValue('');
                    f.findField("MMNAME_E").setValue('');
                }
                else
                {
                    Ext.Ajax.request({
                        url: ChkMmcode,
                        method: reqVal_p,
                        params: { mmcode: field.getValue() },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                var rtnStr = data.msg;
                                if (rtnStr == 'T') {
                                    // 院內碼有在基本檔,可繼續新增儲位碼
                                }
                                else {
                                    // 填入的院內碼不在基本檔
                                    var f = T1Form.getForm();
                                    f.findField("MMCODE").setValue('');
                                    f.findField("MMNAME_C").setValue('');
                                    f.findField("MMNAME_E").setValue('');
                                    Ext.Msg.alert('訊息', '此院內碼未建立基本檔，無法新增儲位碼');
                                }
                            }
                        },
                        failure: function (response, options) {

                        }
                    });
                }
            }
        }
    });

    function setMmcode_Q(args) {
        if (args.MMCODE !== '') {
            T1Query.getForm().findField("P1").setValue(args.MMCODE);
        }
    }

    function setMmcode_I(args) {
        if (args.MMCODE !== '') {
            T1Form.getForm().findField("MMCODE").setValue(args.MMCODE);
            T1Form.getForm().findField("MMNAME_C").setValue(args.MMNAME_C);
            T1Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);
        }
    }

    var MATQueryStore = Ext.create('Ext.data.Store', {
        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: vTaskid
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    T1Query.getForm().findField('P3').setValue(records[0].data.VALUE);
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0043/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, autoLoad: true
    });
    // 查詢欄位
    var mLabelWidth = 60;
    var mWidth = 180;
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
                    xtype: 'combo',
                    store: whnoQueryStore,
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    padding: '0 4 0 4',
                    width: mWidth,
                    allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    listeners: {
                        "select": function (c, r, i, e) {
                            vTaskid = r.get('EXTRA1');
                            MATQueryStore.load();
                        }, boxready: function () {
                            //this.store.load();
                        }
                    }
                }, {
                    xtype: 'combo',
                    store: MATQueryStore,
                    fieldLabel: '物料分類',
                    name: 'P3',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                    padding: '0 4 0 4',
                    width: mWidth,
                    allowBlank: false, // 欄位為必填
                    fieldCls: 'required'
                },
                mmCodeCombo_Q, {
                    xtype: 'button',
                    itemId: 'btnMmcode_Q',
                    iconCls: 'TRASearch',
                    handler: function () {
                        var f = T1Query.getForm();
                        popMmcodeForm(viewport, '/api/AA0043/GetMmcode_Q', { MMCODE: f.findField("P1").getValue() }, setMmcode_Q);
                    }
                }, {
                    xtype: 'textfield',
                    fieldLabel: '儲位代碼',
                    name: 'P2',
                    id: 'P2',
                    width: mWidth,
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        IsSend = false;
                        T1Grid.columns[1].setVisible(false);
                        T1Grid.columns[2].setVisible(false);
                        T1Grid.down('#add').setDisabled(false);
                        T1Grid.down('#edit').setDisabled(true);
                        T1Grid.down('#delete').setDisabled(true);
                        T1Grid.down('#export').setDisabled(false);
                        setCmpShowCondition(false, false, false, false, true, false, false, true, false, false, false, true);
                        Ext.getCmp('import').setDisabled(true);
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        msglabel("");
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.MiWlocinv', { // 定義於/Scripts/app/store/MiWlocinv.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', handler: function () {
                    mmcodeQueryStore.removeAll();
                    T1Set = '/api/AA0043/Create'; // AA0043Controller的Create
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0043/Update';
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0043/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'export', text: '匯出', handler: function () {
                    var p = new Array();
                    p.push({ name: 'FN', value: '儲位碼維護.xls' });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    PostForm(GetExcel, p);
                    msglabel('匯出完成');
                }
            }, {
                xtype: 'filefield',
                name: 'send',
                id: 'send',
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
                        T1Grid.down('#add').setDisabled(false);
                        T1Grid.down('#edit').setDisabled(false);
                        T1Grid.down('#delete').setDisabled(false);
                        T1Grid.down('#export').setDisabled(false);
                        setCmpShowCondition(false, false, false, false, true, false, false, true, false, false, false, true);
                        var files = event.target.files;
                        var self = this; // the controller
                        if (!files || files.length == 0) return; // make sure we got something
                        var f = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '請選擇xlsx或xls檔案！');
                            Ext.getCmp('send').fileInputEl.dom.value = '';
                            msglabel("請選擇xlsx或xls檔案！");
                        }
                        else {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            var formData = new FormData();
                            formData.append("file", f);
                            var ajaxRequest = $.ajax({
                                type: "POST",
                                url: "/api/AA0043/SendExcel",
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
                                        T1Grid.down('#add').setDisabled(false);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T1Store.loadData(data.etts, false);
                                        IsSend = true;
                                        Ext.getCmp('import').setDisabled(false);
                                        T1Grid.columns[1].setVisible(true);
                                        T1Grid.columns[2].setVisible(true);
                                        for (var i = 0; i < T1Grid.getStore().data.items.length; i++) {
                                            if (T1Grid.getStore().data.items[i].data.CHECK_RESULT.indexOf('檢核通過') == '-1') {
                                                Ext.getCmp('import').setDisabled(true);
                                                break;
                                            }
                                        }
                                        T1Grid.down('#add').setDisabled(true);
                                        setCmpShowCondition(true, true, true, false, true, false, false, true, false, false, false, true);
                                    }
                                    T1Grid.down('#edit').setDisabled(true);
                                    T1Grid.down('#delete').setDisabled(true);
                                    T1Grid.down('#export').setDisabled(true);
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('import').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            }, {
                xtype: 'button',
                text: '上傳更新',
                id: 'import',
                name: 'import',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0043/Import',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T1Store.loadData(data.etts, false);
                                Ext.MessageBox.alert("提示", "上傳更新完成");
                                msglabel("訊息區:上傳更新完成");
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
            }, {
                xtype: 'docbutton',
                text: '下載範本',
                documentKey: 'AA0043'
            }
        ]
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        msglabel("");

        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MiWlocinv'); // /Scripts/app/model/MiWlocinv.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("WH_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            w = f.findField("MMCODE"); // 院內碼在新增時才可填寫
            w.setReadOnly(false);
            w.clearInvalid();
            v = f.findField("STORE_LOC"); // 儲位代碼在新增時才可填寫
            v.setReadOnly(false);
            v.clearInvalid();
            y = f.findField('INV_QTY');
            y.setValue(0);

            setCmpShowCondition(false, false, false, true, false, false, true, false, false, true, false, false);
            Ext.getCmp("mmcodeComboSet_I").getComponent('btnMmcode_I').show();
        }
        else {
            setStore_loc(f.findField("WH_NO").getValue(), 'U');
            u = f.findField('INV_QTY');
            setCmpShowCondition(false, false, false, false, true, false, false, true, false, true, false, false);
            Ext.getCmp("mmcodeComboSet_I").getComponent('btnMmcode_I').hide();
        }
        f.findField('x').setValue(x);
        f.findField('STORE_LOC').setReadOnly(false);
        f.findField('INV_QTY').setReadOnly(false);
        f.findField('LOC_NOTE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        // title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            //dock: 'top',
            // xtype: 'toolbar',
            //layout:'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30,
            labelAlign: 'Center'
        }, {
            text: "檢核結果",
            dataIndex: 'CHECK_RESULT',
            width: 150
        }, {
            text: "異動結果",
            dataIndex: 'IMPORT_RESULT',
            width: 85
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            width: 70
        }, {
            text: "庫房名稱",
            dataIndex: 'WH_NAME',
            width: 110
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 250
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 400
        }, {
            text: "儲位代碼",
            dataIndex: 'STORE_LOC',
            width: 80
        }, {
            text: "庫存數量",
            xtype: 'numbercolumn',
            dataIndex: 'INV_QTY',
            width: 80,
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec) {
                    viewport.down('#form').expand();
                    msglabel("");
                }
                setFormT1a();
            }
        }
    });

    function setFormT1a() {
        if ((T1Grid.down('#add').savedDisabled)) {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#delete').setDisabled(true);
        }
        else if (T1Rec == 0) {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#delete').setDisabled(true);
        }
        else {
            T1Grid.down('#edit').setDisabled(false);
            T1Grid.down('#delete').setDisabled(false);
        }

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('WH_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
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
        bodyPadding: '10 0 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        items: [{
            xtype: 'container',
            style: 'border-spacing:0',
            defaultType: 'textfield',
            items: [
                {
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                },
                {
                    xtype: 'displayfield', //白底
                    fieldLabel: '檢核結果',
                    name: 'CHECK_RESULT'
                },
                {
                    xtype: 'displayfield', //白底
                    fieldLabel: '上傳結果',
                    name: 'IMPORT_RESULT'
                },
                {
                    xtype: 'combo',
                    store: whnoQueryStore,
                    fieldLabel: '庫房代碼',
                    name: 'WH_NO',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    blankText: "請選擇庫房代碼",
                    allowBlank: false,
                    readOnly: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            setStore_loc(newValue.data["VALUE"], 'I');
                            clearFormForWhno();
                        }
                    }
                },
                {
                    xtype: 'displayfield', //白色代碼
                    fieldLabel: '庫房代碼',
                    name: 'WH_NAME_DISPLAY',
                }, {
                    fieldLabel: '庫房代碼', //紅底顯示
                    name: 'WH_NAME_TEXT',
                    enforceMaxLength: true,
                    maxLength: 100,
                    readOnly: true,
                    fieldCls: 'required'
                }, ,
                {
                    xtype: 'displayfield', //白色代碼
                    fieldLabel: '庫房名稱',
                    name: 'WH_NAME'
                }, {
                    xtype: 'container',
                    layout: 'hbox',
                    //padding: '0 0 7 0',
                    id: 'mmcodeComboSet_I',
                    items: [
                        mmCodeCombo_I,
                        {
                            xtype: 'button',
                            itemId: 'btnMmcode_I',
                            iconCls: 'TRASearch',
                            handler: function () {
                                var f = T1Form.getForm();
                                popMmcodeForm(viewport, '/api/AA0043/GetMmcode_I', {
                                    MMCODE: f.findField("MMCODE").getValue(),
                                    WH_NO: f.findField("WH_NO").getValue()
                                }, setMmcode_I);
                            }
                        }]
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '院內碼',
                    name: 'MMCODE_DISPLAY'
                },
                {
                    fieldLabel: '院內碼',
                    name: 'MMCODE_TEXT',
                    enforceMaxLength: true,
                    maxLength: 100,
                    readOnly: true,
                    fieldCls: 'required'
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '中文品名',
                    name: 'MMNAME_C'
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '英文品名',
                    name: 'MMNAME_E'
                }, {
                    xtype: 'combo',
                    store: Store_locStore,
                    fieldLabel: '儲位代碼',
                    name: 'STORE_LOC',
                    displayField: 'VALUE',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    blankText: "請選擇儲位代碼",
                    allowBlank: false,
                    readOnly: true,
                    fieldCls: 'required',
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div></tpl>'
                }, {
                    xtype: 'displayfield', //白底顯示
                    fieldLabel: '儲位代碼',
                    name: 'STORE_LOC_DISPLAY',
                    submitValue: true
                }, {
                    fieldLabel: '儲位代碼', //紅底顯示
                    name: 'STORE_LOC_TEXT',
                    enforceMaxLength: true,
                    maxLength: 20,
                    readOnly: true,
                    fieldCls: 'required'
                }, {
                    xtype: "numberfield",
                    fieldLabel: '庫存數量',
                    name: 'INV_QTY',
                    enforceMaxLength: true,
                    maxLength: 15,
                    readOnly: true,
                    allowBlank: true, // 欄位為必填
                    hideTrigger: true,
                    minValue: 0,
                    negativeText: "庫存數量不可為負數",
                    decimalPrecision: 2,
                    allowDecimals: true,
                    forcePrecision: true
                }, {
                    xtype: 'textareafield',
                    fieldLabel: '備註',
                    name: 'LOC_NOTE',
                    enforceMaxLength: true,
                    maxLength: 100,
                    height: 200,
                    readOnly: true
                }]
        }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                
                if ((T1Form.getForm().findField("WH_NO").getValue() == "") ||
                    (T1Form.getForm().findField("WH_NO").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>庫房代碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>庫房代碼不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("MMCODE").getValue() == "") ||
                    (T1Form.getForm().findField("MMCODE").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("STORE_LOC").getValue() == "") ||
                    (T1Form.getForm().findField("STORE_LOC").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>儲位代碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>儲位代碼不可為空</span>，請重新輸入。");
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
                        fieldNames = [];
                        fields = this.up('form').getInvalidFields();
                        for (var i = 0; i < fields.length; i++) {
                            field = fields[i];
                            fieldNames.push(field.getName());
                        }
                        console.log(fieldNames);

                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel(" 輸入資料格式有誤");
                    }
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
        ,
        getInvalidFields: function () {
            var invalidFields = [];
            Ext.suspendLayouts();
            this.form.getFields().filterBy(function (field) {
                if (field.validate()) return;
                invalidFields.push(field);
            });
            Ext.resumeLayouts(true);
            return invalidFields;
        },    
    });

    function clearFormForWhno() {
        var f = T1Form.getForm();
        a = f.findField("MMCODE");
        a.setValue("");
        a.clearInvalid();
        b = f.findField("MMNAME_C");
        b.setValue("");
        b.clearInvalid();
        c = f.findField("MMNAME_E");
        c.setValue("");
        c.clearInvalid();
        d = f.findField("STORE_LOC");
        d.setValue("");
        d.clearInvalid();
        e = f.findField("INV_QTY");
        e.setValue(0);
    }

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition(false, false, false, false, true, false, false, true, false, false, false, true);
                    Ext.getCmp("mmcodeComboSet_I").getComponent('btnMmcode_I').hide();
                    T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var whnovalue = T1Form.getForm().findField("WH_NO").getValue();
                    var mmcodevalue = T1Form.getForm().findField("MMCODE").getValue();
                    var store_locvalue = T1Form.getForm().findField("STORE_LOC").getValue();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            msglabel(" 資料新增成功");
                            r.commit();
                            T1Query.getForm().findField("P0").setValue(whnovalue);
                            T1Query.getForm().findField("P1").setValue(mmcodevalue);
                            T1Query.getForm().findField("P2").setValue(store_locvalue);
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            msglabel(" 資料修改成功");
                            r.commit();
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            msglabel(" 資料刪除成功");
                            r.commit();
                            break;
                    }
                    myMask.hide();
                    viewport.down('#form').setCollapsed("true");
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
        viewport.down('#form').setTitle('瀏覽');
        viewport.down('#form').setCollapsed("true");
        setFormT1a();
        setCmpShowCondition(false, false, false, false, true, false, false, true, false, false, false, true);
        Ext.getCmp("mmcodeComboSet_I").getComponent('btnMmcode_I').hide();
    }

    function showComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.show();
    }

    function hideComponent(form, fieldName) {
        var u = form.findField(fieldName);
        u.hide();
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
    });

    function setCmpShowCondition(check_result, import_result, wh_name, wh_no, wh_name_display, wh_name_text, mmcode, mmcode_display, mmcode_text, store_loc, store_loc_display, store_loc_text) {
        var f = T1Form.getForm();
        check_result ? showComponent(f, "CHECK_RESULT") : hideComponent(f, "CHECK_RESULT");
        import_result ? showComponent(f, "IMPORT_RESULT") : hideComponent(f, "IMPORT_RESULT");
        wh_name ? showComponent(f, "WH_NAME") : hideComponent(f, "WH_NAME");
        wh_no ? showComponent(f, "WH_NO") : hideComponent(f, "WH_NO");
        wh_name_display ? showComponent(f, "WH_NAME_DISPLAY") : hideComponent(f, "WH_NAME_DISPLAY");
        wh_name_text ? showComponent(f, "WH_NAME_TEXT") : hideComponent(f, "WH_NAME_TEXT");
        mmcode ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        mmcode_display ? showComponent(f, "MMCODE_DISPLAY") : hideComponent(f, "MMCODE_DISPLAY");
        mmcode_text ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
        store_loc ? showComponent(f, "STORE_LOC") : hideComponent(f, "STORE_LOC");
        store_loc_display ? showComponent(f, "STORE_LOC_DISPLAY") : hideComponent(f, "STORE_LOC_DISPLAY");
        store_loc_text ? showComponent(f, "STORE_LOC_TEXT") : hideComponent(f, "STORE_LOC_TEXT");
    }

    setCmpShowCondition(false, false, false, false, true, false, false, true, false, false, false, true);
    T1Grid.columns[1].setVisible(false);
    T1Grid.columns[2].setVisible(false);
    Ext.getCmp("mmcodeComboSet_I").getComponent('btnMmcode_I').hide();
    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格
    setComboData();
});
