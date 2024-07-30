Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AB0042/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "PCA固定處方維護作業";

    var T1Rec = 0;
    var T1LastRec = null;

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

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P0',
        id: 'P0',
        fieldLabel: 'PCA固定處方頭代碼',
        allowBlank: true,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0042/GetMMCodeCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T1QueryForm.getForm();
                if (r.get('P0') !== '') {
                    f.findField("P0").setValue(r.get('MMCODE'));
                    f.findField("P1").setValue(r.get('MMNAME_E'));
                }
            }
        }
    });

    //新增院內碼
    var mmCodeCombo_i = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'PCACODE',
        fieldLabel: 'PCA固定處方頭代碼',
        fieldCls: 'required',
        allowBlank: false,
        //labelWidth: 80,
        labelAlign: 'right',
        //width: 220,
        blankText: "請輸入PCA固定處方頭代碼",

        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0042/GetMMCodeCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("PCACODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("E_ORDERUNIT").setValue(r.get('E_ORDERUNIT'));
                    f.findField("E_PATHNO").setValue(r.get('E_PATHNO'));
                }
            }
        }
    });

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T1QueryForm.getForm().findField("P0").setValue(args.MMCODE);
            T1QueryForm.getForm().findField("P1").setValue(args.MMNAME_E);
        }
    }

    function setMmcode_i(args) {
        if (args.MMCODE !== '') {
            T1Form.getForm().findField("PCACODE").setValue(args.MMCODE);
            T1Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);

            Ext.Ajax.request({
                url: '../../../api/AB0042/GetE_ORDERUNIT',
                method: reqVal_p,
                params: {
                    MMCODE: args.MMCODE
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        var datas = data.etts;
                        T1Form.getForm().findField("E_ORDERUNIT").setValue(datas[0].E_ORDERUNIT);
                        T1Form.getForm().findField("E_PATHNO").setValue(datas[0].E_PATHNO);
                    }
                },
                failure: function (response, options) {
                }
            });
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
        bodyPadding: '10 0 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 120,
            width: 280
        },
        items: [
            {
                xtype: 'container',
                style: 'border-spacing:0',
                defaultType: 'textfield',
                items: [{
                    xtype: 'container',
                    layout: 'hbox',
                    id: 'mmcodeComboSet',
                    items: [
                        mmCodeCombo,
                        {
                            xtype: 'button',
                            itemId: 'btnMmcode',
                            iconCls: 'TRASearch',
                            handler: function () {
                                var f = T1QueryForm.getForm();
                                popMmcodeForm(viewport, '/api/AB0042/GetMmcode', { MMCODE: f.findField("P0").getValue() }, setMmcode);
                            }
                        }

                    ]
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '英文品名',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 200 // 可輸入最大長度為200
                }
                ]
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
                T1QueryForm.getForm().findField("P0").setValue("");
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
            }
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.ME_PCAM', { // 定義於/Scripts/app/store/ME_PCAM.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue()
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
                itemId: 'add', text: '新增', disabled: false, handler: function () {
                    T1Set = '../../../api/AB0042/CreateM';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/AB0042/UpdateM';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AB0042/DeleteM';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
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
        bodyPadding: '10 0 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 120,
            width: 280
        },
        items: [
            {
                xtype: 'container',
                style: 'border-spacing:0',
                defaultType: 'textfield',
                items: [{
                    fieldLabel: 'Update',
                    name: 'x',
                    xtype: 'hidden'
                }, {
                    xtype: 'container',
                    layout: 'hbox',
                    //padding: '10 0 7 0',
                    id: 'mmcodeComboSet_i',
                    items: [
                        mmCodeCombo_i,
                        {
                            xtype: 'button',
                            itemId: 'btnMmcode_i',
                            iconCls: 'TRASearch',
                            handler: function () {
                                var f = T1Form.getForm();
                                popMmcodeForm(viewport, '/api/AB0042/GetMmcode', { MMCODE: f.findField("PCACODE").getValue() }, setMmcode_i);
                            }
                        }

                    ]
                }, {
                    xtype: 'displayfield',
                    fieldLabel: 'PCA固定處方頭代碼', //白底顯示
                    name: 'PCACODE_DISPLAY',
                    readOnly: true
                }, {
                    fieldLabel: 'PCA固定處方頭代碼',//紅底顯示
                    name: 'PCACODE_TEXT',
                    readOnly: true,
                    fieldCls: 'required'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '英文品名', //白底顯示
                    name: 'MMNAME_E',
                    readOnly: true,
                    submitValue: true
                }, {
                    xtype: "numberfield",
                    fieldLabel: '劑量',
                    name: 'DOSE',
                    enforceMaxLength: true,
                    maxLength: 15,
                    readOnly: true,
                    allowBlank: true, // 欄位為非必填
                    hideTrigger: true,
                    decimalPrecision: 2,
                    allowDecimals: true,
                    forcePrecision: true
                }, {
                    scrollable: true,
                    fieldLabel: '院內頻率',
                    name: 'FREQNO',
                    enforceMaxLength: true,
                    maxLength: 15,
                    readOnly: true,
                    allowBlank: true // 欄位為非必填
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '使用途徑', //白底顯示
                    name: 'E_PATHNO',
                    readOnly: true,
                    submitValue: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '醫囑單位', //白底顯示
                    name: 'E_ORDERUNIT',
                    readOnly: true,
                    submitValue: true
                }]
            }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if ((T1Form.getForm().findField("PCACODE").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>PCA固定處方頭代碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>PCA固定處方頭代碼不可為空</span>，請重新輸入。");
                }
                else {
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
            text: "PCA固定處方頭",
            dataIndex: 'PCACODE',
            width: 100
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "劑量",
            dataIndex: 'DOSE',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "院內頻率",
            dataIndex: 'FREQNO',
            width: 80
        }, {
            text: "使用途徑",
            dataIndex: 'E_PATHNO',
            width: 60
        }, {
            text: "醫囑單位",
            dataIndex: 'E_ORDERUNIT',
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
                    msglabel("");

                }
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

        var f = T1Form.getForm();
        msglabel("");
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_PCAM'); // /Scripts/app/model/ME_PCAM.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField('PCACODE');
            u.setReadOnly(false);
            u.clearInvalid();

            setCmpShowCondition(true, false, false);
            Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').show();
        }
        else {

            u = f.findField('DOSE');
            setCmpShowCondition(false, true, false);
            Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
        }

        f.findField('x').setValue(x);
        f.findField('DOSE').setReadOnly(false);
        f.findField('FREQNO').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
    }

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            if (T1Form.getForm().findField("DOSE").getValue() == null) {
                T1Form.getForm().findField("DOSE").setValue(0.00);
            }
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition(false, false, true);
                    Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
                    //T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var pcacodevalue = T1Form.getForm().findField("PCACODE").getValue();
                    var mmname_evalue = T1Form.getForm().findField("MMNAME_E").getValue();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel(" 資料新增成功");
                            T1QueryForm.getForm().findField("P0").setValue(pcacodevalue);
                            T1QueryForm.getForm().findField("P1").setValue(mmname_evalue);
                            break;
                        case "U":
                            msglabel(" 資料修改成功");
                            break;
                        case "D":
                            //T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            msglabel(" 資料刪除成功");
                            //r.commit();
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
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0);
        T2Grid.down('#add').setDisabled(T1Rec === 0);

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');

            TATabs.setActiveTab('Form');
            T2Grid.down('#edit').setDisabled(true);
            T2Grid.down('#delete').setDisabled(true);
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
        T2Grid.down('#add').setDisabled(T1Rec === 1);
        setCmpShowCondition(false, false, true);
        Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
        setFormT1a();
    }

    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var CONSUMEFLAG_Store = Ext.create('Ext.data.Store', {  //扣庫的store
        fields: ['VALUE', 'TEXT']
    });
    var COMPUTECODE_Store = Ext.create('Ext.data.Store', {  //計費規則的store
        fields: ['VALUE', 'TEXT']
    });

    function SetCONSUMEFLAG() { //建立扣庫的下拉式選單
        CONSUMEFLAG_Store.add({ VALUE: '', TEXT: '' });
        CONSUMEFLAG_Store.add({ VALUE: 'Y', TEXT: 'Y 是' });
        CONSUMEFLAG_Store.add({ VALUE: 'N', TEXT: 'N 否' });

    }
    function SetCOMPUTECODE() { //建立計費規則的下拉式選單
        COMPUTECODE_Store.add({ VALUE: '', TEXT: '' });
        COMPUTECODE_Store.add({ VALUE: 'Y', TEXT: 'Y 計費' });
        COMPUTECODE_Store.add({ VALUE: 'N', TEXT: 'N 不計費' });
        COMPUTECODE_Store.add({ VALUE: 'C', TEXT: 'C 不申報' });
    }

    //新增院內碼
    var mmCodeCombo_i_D = Ext.create('WEBAPP.form.MMCodeCombo', {
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
        queryUrl: '/api/AB0042/GetMMCodeCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("E_ORDERUNIT").setValue(r.get('E_ORDERUNIT'));
                }
            }
        }
    });

    function setMmcode_i_D(args) {
        if (args.MMCODE !== '') {
            T2Form.getForm().findField("MMCODE").setValue(args.MMCODE);
            T2Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);
            Ext.Ajax.request({
                url: '../../../api/AB0042/GetE_ORDERUNIT',
                method: reqVal_p,
                params: {
                    MMCODE: args.MMCODE
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        var datas = data.etts;
                        T2Form.getForm().findField("E_ORDERUNIT").setValue(datas[0].E_ORDERUNIT);
                    }
                },
                failure: function (response, options) {
                }
            });
        }
    }

    var T2Store = Ext.create('WEBAPP.store.ME_PCAD', { // 定義於/Scripts/app/store/ME_PCAD.js
        listeners: {
            //載入前將master的PCA固定處方頭關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("PCACODE_TEXT").getValue()
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
                    T2Set = '../../../api/AB0042/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/AB0042/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T2Set = '../../../api/AB0042/DeleteD';
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit();
                        }
                    }
                    );
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
        bodyPadding: '10 0 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 65,
            width: 280
        },
        defaultType: 'textfield',
        items: [{
            xtype: 'container',
            style: 'border-spacing:0',
            defaultType: 'textfield',
            items: [{
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                fieldLabel: 'PCA固定處方頭',
                name: 'PCACODE',
                xtype: 'hidden'
            }, {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 0 7 0',
                id: 'mmcodeComboSet_i_D',
                items: [
                    mmCodeCombo_i_D,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode_i_D',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            popMmcodeForm(viewport, '/api/AB0042/GetMmcode', { MMCODE: f.findField("MMCODE").getValue() }, setMmcode_i_D);
                        }
                    }

                ]
            }, {
                xtype: 'displayfield',
                fieldLabel: '院內碼', //白底顯示
                name: 'MMCODE_DISPLAY',
                readOnly: true
            }, {
                fieldLabel: '院內碼',//紅底顯示
                name: 'MMCODE_TEXT',
                readOnly: true,
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名', //白底顯示
                name: 'MMNAME_E',
                readOnly: true,
                submitValue: true
            }, {
                xtype: "numberfield",
                fieldLabel: '劑量',
                name: 'DOSE',
                enforceMaxLength: true,
                maxLength: 15,
                readOnly: true,
                allowBlank: true, // 欄位為非必填
                hideTrigger: true,
                decimalPrecision: 2,
                allowDecimals: true,
                forcePrecision: true
            }, {
                xtype: 'combo',
                fieldLabel: '扣庫',
                name: 'CONSUMEFLAG',
                id: 'CONSUMEFLAG',
                store: CONSUMEFLAG_Store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true
            }, {
                fieldLabel: '扣庫', //白框顯示
                name: 'CONSUMEFLAG_TEXT',
                readOnly: true
            }, {
                xtype: 'combo',
                fieldLabel: '計費規則',
                name: 'COMPUTECODE',
                id: 'COMPUTECODE',
                store: COMPUTECODE_Store,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE',
                autoSelect: true,
                anyMatch: true
            }, {
                fieldLabel: '計費規則', //白框顯示
                name: 'COMPUTECODE_TEXT',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '醫囑單位', //白底顯示
                name: 'E_ORDERUNIT',
                readOnly: true,
                submitValue: true
            }]
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("MMCODE").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else {
                    if (this.up('form').getForm().isValid()) { // 檢查T2Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        }
                        );
                    }
                    else {
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                        msglabel(" 輸入資料格式有誤");

                    }
                }

            }
        }, {
            itemId: 'T2Cancel', text: '取消', hidden: true, handler: function () {
                T2Cleanup();
                msglabel("");
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
        msglabel("");
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('WEBAPP.model.ME_PCAD'); //Scripts / app / model / ME_PCAD.js
            T2Form.loadRecord(r);

            f2.findField("PCACODE").setValue(T1Form.getForm().findField("PCACODE").getValue());
            u = f2.findField("MMCODE");
            u.setReadOnly(false);
            u.clearInvalid();

            setCmpShowCondition_D(true, false, false, true, false, true, false);
            Ext.getCmp("mmcodeComboSet_i_D").getComponent('btnMmcode_i_D').show();
        }
        else {
            u = f2.findField('DOSE');
            setCmpShowCondition_D(false, true, false, true, false, true, false);
            Ext.getCmp("mmcodeComboSet_i_D").getComponent('btnMmcode_i_D').hide();
        }

        f2.findField('x').setValue(x);
        f2.findField('DOSE').setReadOnly(false);
        f2.findField('CONSUMEFLAG').setReadOnly(false);
        f2.findField('COMPUTECODE').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T2Grid = Ext.create('Ext.grid.Panel', {
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
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 250,
            sortable: true
        }, {
            text: "劑量",
            dataIndex: 'DOSE',
            width: 80,
            sortable: true,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "扣庫",
            dataIndex: 'CONSUMEFLAG_DISPLAY',
            width: 60,
            sortable: true
        }, {
            text: "計費規則",
            dataIndex: 'COMPUTECODE_DISPLAY',
            width: 60,
            sortable: true
        }, {
            text: "醫囑單位",
            dataIndex: 'E_ORDERUNIT',
            width: 60,
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

            f.findField('x').setValue('U');
            var u = f.findField('PCACODE');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            T2Grid.down('#edit').setDisabled(T2Rec === 0);
            T2Grid.down('#delete').setDisabled(T2Rec === 0);

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
            if (T2Form.getForm().findField("CONSUMEFLAG").getValue() == "") {
                T2Form.getForm().findField("CONSUMEFLAG").setValue('Y');
            }
            if (T2Form.getForm().findField("COMPUTECODE").getValue() == "") {
                T2Form.getForm().findField("COMPUTECODE").setValue('C');
            }
            if (T2Form.getForm().findField("DOSE").getValue() == null) {
                T2Form.getForm().findField("DOSE").setValue(0.00);
            }

            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition_D(false, false, true, false, true, false, true);
                    Ext.getCmp("mmcodeComboSet_i_D").getComponent('btnMmcode_i_D').hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            // var v = action.result.etts[0];
                            // r.set(v);
                            // T2Store.insert(0, r);
                            msglabel(" 資料新增成功");
                            //r.commit();
                            break;
                        case "U":
                            // var v = action.result.etts[0];
                            // r.set(v);
                            msglabel(" 資料修改成功");
                            //r.commit();
                            break;
                        case "D":
                            //T2Store.remove(r);
                            msglabel(" 資料刪除成功");
                            // r.commit();
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
        setCmpShowCondition_D(false, false, true, false, true, false, true);
        Ext.getCmp("mmcodeComboSet_i_D").getComponent('btnMmcode_i_D').hide();
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

    //控制不可更改項目的顯示
    function setCmpShowCondition(PCACODE, PCACODE_DISPLAY, PCACODE_TEXT) {
        var f = T1Form.getForm();
        PCACODE ? showComponent(f, "PCACODE") : hideComponent(f, "PCACODE");
        PCACODE_DISPLAY ? showComponent(f, "PCACODE_DISPLAY") : hideComponent(f, "PCACODE_DISPLAY");
        PCACODE_TEXT ? showComponent(f, "PCACODE_TEXT") : hideComponent(f, "PCACODE_TEXT");
    }
    function setCmpShowCondition_D(MMCODE, MMCODE_DISPLAY, MMCODE_TEXT, CONSUMEFLAG, CONSUMEFLAG_TEXT, COMPUTECODE, COMPUTECODE_TEXT) {
        var f = T2Form.getForm();
        MMCODE ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        MMCODE_DISPLAY ? showComponent(f, "MMCODE_DISPLAY") : hideComponent(f, "MMCODE_DISPLAY");
        MMCODE_TEXT ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
        CONSUMEFLAG ? showComponent(f, "CONSUMEFLAG") : hideComponent(f, "CONSUMEFLAG");
        CONSUMEFLAG_TEXT ? showComponent(f, "CONSUMEFLAG_TEXT") : hideComponent(f, "CONSUMEFLAG_TEXT");
        COMPUTECODE ? showComponent(f, "COMPUTECODE") : hideComponent(f, "COMPUTECODE");
        COMPUTECODE_TEXT ? showComponent(f, "COMPUTECODE_TEXT") : hideComponent(f, "COMPUTECODE_TEXT");
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

    setCmpShowCondition(false, false, true);
    setCmpShowCondition_D(false, false, true, false, true, false, true);
    Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
    Ext.getCmp("mmcodeComboSet_i_D").getComponent('btnMmcode_i_D').hide();
    T1QueryForm.getForm().findField('P0').focus();
    SetCONSUMEFLAG();
    SetCOMPUTECODE();
});
