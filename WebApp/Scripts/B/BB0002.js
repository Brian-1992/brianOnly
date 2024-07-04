Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/BB0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "寄售品基本資料";

    var T1Rec = 0;
    var T1LastRec = null;
    var T1GetExcel = '/api/BB0002/Excel';
    var T1GetExcel_D = '/api/BB0002/Excel_D';

    var GetAGEN_NO = '../../../api/BB0002/GetAGEN_NO';
    var GetINIDNAME = '../../../api/BB0002/GetINIDNAME';
    var GetAGENNO = '../../../api/BB0002/GetAgenno';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var AGEN_NO_QStore = Ext.create('Ext.data.Store', {  //查詢廠商碼的store
        fields: ['VALUE', 'TEXT']
    });
    var AGEN_NOStore = Ext.create('Ext.data.Store', {  //新增修改廠商碼的store
        fields: ['VALUE', 'TEXT']
    });
    var INIDNAMEStore = Ext.create('Ext.data.Store', {  //寄放責任地點的store
        fields: ['VALUE', 'TEXT', 'INIDNAME_TEXT']
    });
    var STATUS_MStore = Ext.create('Ext.data.Store', {  //Master的狀態store
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
        queryUrl: '/api/BB0002/GetMMCodeCombo',

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

    //新增院內碼
    var mmCodeCombo_i = Ext.create('WEBAPP.form.MMCodeCombo', {
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
        queryUrl: '/api/BB0002/GetMMCodeCombo',

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    SetAGENNO(r.get('MMCODE'));
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
                if (data.success) {
                    var agen_nos = data.etts;
                    if (agen_nos.length > 0) {
                        AGEN_NO_QStore.add({ VALUE: '', TEXT: '全部' });
                        AGEN_NOStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < agen_nos.length; i++) {
                            AGEN_NO_QStore.add({ VALUE: agen_nos[i].VALUE, TEXT: agen_nos[i].TEXT });
                            AGEN_NOStore.add({ VALUE: agen_nos[i].VALUE, TEXT: agen_nos[i].TEXT });
                        }
                    }
                    T1QueryForm.getForm().findField("P0").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetINIDNAME() { //建立寄放責任中心的下拉式選單
        Ext.Ajax.request({
            url: GetINIDNAME,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var inidnames = data.etts;
                    if (inidnames.length > 0) {
                        INIDNAMEStore.add({ VALUE: '', TEXT: '', INIDNAME_TEXT: '' });
                        for (var i = 0; i < inidnames.length; i++) {
                            INIDNAMEStore.add({ VALUE: inidnames[i].VALUE, TEXT: inidnames[i].TEXT, INIDNAME_TEXT: inidnames[i].INIDNAME_TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetAGENNO(MMCODE) { //依院內碼帶出廠商碼
        Ext.Ajax.request({
            url: GetAGENNO,
            method: reqVal_p,
            params: {
                MMCODE: MMCODE
            },
            success: function (response) {
                var data = response.responseText.replace(new RegExp('"', 'g'), '').split(',');
                T1Form.getForm().findField('AGEN_NO').setValue(data[0]);
                T1Form.getForm().findField('AGEN_NAMEC').setValue(data[1]);
            },
            failure: function (response, options) {
            }
        });
    }

    function SetSTATUS_M() { //建立Master的狀態的下拉式選單
        STATUS_MStore.add({ VALUE: 'A', TEXT: 'A 寄放中' });
        STATUS_MStore.add({ VALUE: 'D', TEXT: 'D 刪除' });
    }

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T1QueryForm.getForm().findField("P1").setValue(args.MMCODE);
            T1QueryForm.getForm().findField("P2").setValue(args.MMNAME_C);
            T1QueryForm.getForm().findField("P3").setValue(args.MMNAME_E);
        }
    }

    function setMmcode_i(args) {
        if (args.MMCODE !== '') {
            T1Form.getForm().findField("MMCODE").setValue(args.MMCODE);
            T1Form.getForm().findField("MMNAME_C").setValue(args.MMNAME_C);
            T1Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);
            SetAGENNO(args.MMCODE);
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
            labelWidth: 80,
            width: 300
        },
        items: [
            {
                xtype: 'container',
                style: 'border-spacing:0',
                defaultType: 'textfield',
                items: [
                    {
                        xtype: 'combo',
                        fieldLabel: '廠商碼',
                        name: 'P0',
                        id: 'P0',
                        store: AGEN_NO_QStore,
                        queryMode: 'local',
                        displayField: 'TEXT',
                        valueField: 'VALUE',
                        autoSelect: true,
                        anyMatch: true,
                        tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                    }, {
                        xtype: 'container',
                        layout: 'hbox',
                        padding: '0 0 5 0',
                        id: 'mmcodeComboSet',
                        items: [
                            mmCodeCombo,
                            {
                                xtype: 'button',
                                itemId: 'btnMmcode',
                                iconCls: 'TRASearch',
                                handler: function () {
                                    var f = T1QueryForm.getForm();
                                    popMmcodeForm(viewport, '/api/BB0002/GetMmcode', { MMCODE: f.findField("P1").getValue() }, setMmcode);
                                }
                            }

                        ]
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
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '交易日期區間',
                        name: 'P4',
                        id: 'P4',
                        enforceMaxLength: true,
                        allowBlank: true, // 欄位是否為必填
                        //format: 'X/m/d',
                        renderer: function (value, meta, record) {
                            return Ext.util.Format.date(value, 'X/m/d');
                        }
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        name: 'P5',
                        id: 'P5',
                        enforceMaxLength: true,
                        allowBlank: true, // 欄位是否為必填
                        //format: 'X/m/d',
                        renderer: function (value, meta, record) {
                            return Ext.util.Format.date(value, 'X/m/d');
                        }
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

    var T1Store = Ext.create('WEBAPP.store.PH_PUT_M', { // 定義於/Scripts/app/store/PH_PUT_M.js
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
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', disabled: false, handler: function () {
                    T1Set = '../../../api/BB0002/CreateM';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/BB0002/UpdateM';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'export', text: '匯出', disabled: false, handler: function () {
                    var p = new Array();
                    TATabs.setActiveTab('Query');
                    p.push({ name: 'FN', value: '寄售品基本資料維護.xls' });
                    p.push({ name: 'p0', value: T1QueryForm.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1QueryForm.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1QueryForm.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1QueryForm.getForm().findField('P3').getValue() });
                    PostForm(T1GetExcel, p);
                    msglabel('匯出完成');
                }
            },
            {
                itemId: 'export_D', text: '帳務明細匯出', handler: function () {
                    var p = new Array();
                    TATabs.setActiveTab('Query');
                    p.push({ name: 'FN', value: '寄售品帳務明細匯出.xls' });
                    p.push({ name: 'p4', value: T1QueryForm.getForm().findField('P4').rawValue });
                    p.push({ name: 'p5', value: T1QueryForm.getForm().findField('P5').rawValue });
                    PostForm(T1GetExcel_D, p);
                    msglabel('帳務明細匯出完成');
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
            labelWidth: 90,
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
                    fieldLabel: '廠商代碼',
                    name: 'AGEN_NO',
                    xtype: 'hidden',
                    submitValue: true
                }, {
                    xtype: 'container',
                    layout: 'hbox',
                    //padding: '0 0 7 0',
                    id: 'mmcodeComboSet_i',
                    items: [
                        mmCodeCombo_i,
                        {
                            xtype: 'button',
                            itemId: 'btnMmcode_i',
                            iconCls: 'TRASearch',
                            handler: function () {
                                var f = T1Form.getForm();
                                popMmcodeForm(viewport, '/api/BB0002/GetMmcode', { MMCODE: f.findField("MMCODE").getValue() }, setMmcode_i);
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
                    fieldLabel: '廠商碼', //白底顯示
                    name: 'AGEN_NAMEC',
                    readOnly: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '中文品名', //白底顯示
                    name: 'MMNAME_C',
                    readOnly: true,
                    submitValue: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '英文品名', //白底顯示
                    name: 'MMNAME_E',
                    readOnly: true,
                    submitValue: true
                }, {
                    xtype: 'combo',
                    store: INIDNAMEStore,
                    fieldLabel: '寄放責任中心',
                    name: 'DEPT',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    matchFieldWidth: true,
                    blankText: "請選擇寄放責任中心",
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    readOnly: true,
                    fieldCls: 'required',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            T1Form.getForm().findField("DEPTNAME").setValue(newValue.data.INIDNAME_TEXT);
                        }
                    }
                }, {
                    fieldLabel: '寄放責任中心',//紅底顯示
                    name: 'DEPT_NAME_TEXT',
                    readOnly: true,
                    fieldCls: 'required'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '寄放責任中心', //白底顯示
                    name: 'DEPT_NAME_DISPLAY',
                    readOnly: true
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '寄放地點', //白底顯示
                    name: 'DEPTNAME',
                    readOnly: true,
                    submitValue: true
                }, {
                    xtype: "numberfield",
                    fieldLabel: '同意寄放量',
                    name: 'QTY',
                    enforceMaxLength: true,
                    maxLength: 15,
                    readOnly: true,
                    allowBlank: false, // 欄位為必填
                    hideTrigger: true,
                    blankText: "請輸入同意寄放量",
                    fieldCls: 'required'
                }, {
                    xtype: 'displayfield',
                    fieldLabel: '同意寄放量', //白底顯示
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
                    store: STATUS_MStore,
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
                    allowBlank: false,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    readOnly: true,
                    fieldCls: 'required'
                }, {
                    fieldLabel: '狀態',//紅底顯示
                    name: 'STATUS_TEXT',
                    readOnly: true,
                    fieldCls: 'required'
                }]
            }], //text: ["13456"],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if ((T1Form.getForm().findField("AGEN_NO").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>廠商碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>廠商碼不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("MMCODE").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("DEPT").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>寄放責任中心不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>寄放責任中心不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("QTY").getValue()) < 0) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>同意寄放量需大於等於0</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>同意寄放量需大於等於0</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("STATUS").getValue().trim()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>狀態不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>狀態不可為空</span>，請重新輸入。");
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
            text: "廠商碼",
            dataIndex: 'AGEN_NAMEC',
            width: 180
        }, {
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
            text: "同意寄放量",
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
            itemclick: function (self, record, item, index, e, eOpts) {
                T1Rec = index;
                T1LastRec = record;
                setFormT1a();
                if (T1LastRec) {
                    msglabel("");
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
            var r = Ext.create('WEBAPP.model.PH_PUT_M'); // /Scripts/app/model/MiWlocinv.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("MMCODE"); // 院內碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('DEPT').clearInvalid();
            f.findField('DEPT').setReadOnly(false);
            f.findField('QTY').clearInvalid();
            f.findField('MEMO').clearInvalid();
            f.findField('STATUS').setValue("A");

            setCmpShowCondition(true, false, false, true, false, false, true, false, true, false);
            Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').show();
        }
        else {

            u = f.findField('QTY');
            setCmpShowCondition(false, true, false, false, false, true, false, true, true, false);
            Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
        }

        f.findField('x').setValue(x);
        f.findField('QTY').setReadOnly(false);
        f.findField('MEMO').setReadOnly(false);
        f.findField('STATUS').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
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
                    setCmpShowCondition(false, true, false, false, false, true, false, true, false, true);
                    Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
                    T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var agennovalue = T1Form.getForm().findField("AGEN_NO").getValue();
                    var mmcodevalue = T1Form.getForm().findField("MMCODE").getValue();
                    var mmname_cvalue = T1Form.getForm().findField("MMNAME_C").getValue();
                    var mmname_evalue = T1Form.getForm().findField("MMNAME_E").getValue();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            msglabel(" 資料新增成功");
                            r.commit();
                            T1QueryForm.getForm().findField("P0").setValue(agennovalue);
                            T1QueryForm.getForm().findField("P1").setValue(mmcodevalue);
                            T1QueryForm.getForm().findField("P2").setValue(mmname_cvalue);
                            T1QueryForm.getForm().findField("P3").setValue(mmname_evalue);
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

        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            /*var f = T1Form.getForm();
            f.findField('x').setValue('U');*/

            TATabs.setActiveTab('Form');
            if (T1Form.getForm().findField("STATUS").getValue() == "D") {
                T2Grid.down('#add').setDisabled(T1Rec === 1);
            }
            else {
                T2Grid.down('#add').setDisabled(T1Rec === 0);
            }
            T2Grid.down('#edit').setDisabled(true);
            //T2Grid.down('#delete').setDisabled(true);
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
        setFormT1a();
        setCmpShowCondition(false, true, false, false, false, true, false, true, false, true);
        Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
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
        EXTYPE_Store.add({ VALUE: '20', TEXT: '20 耗用' });
    }
    function SetSTATUS_D() { //建立異動類別的下拉式選單
        STATUS_DStore.add({ VALUE: 'A', TEXT: 'A 未處理' });
        STATUS_DStore.add({ VALUE: 'B', TEXT: 'B 已轉檔' });
    }

    var T2Store = Ext.create('WEBAPP.store.PH_PUT_D', { // 定義於/Scripts/app/store/PH_PUT_D.js
        listeners: {
            //載入前將master的院內碼關聯帶入
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("AGEN_NO").getValue(),
                    p1: T1Form.getForm().findField("MMCODE").getValue(),
                    p2: T1Form.getForm().findField("DEPT").getValue(),
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
                    T2Set = '../../../api/BB0002/CreateD';
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/BB0002/UpdateD';
                    setFormT2("U", '修改');
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
            fieldLabel: '交易流水號',
            name: 'SEQ',
            xtype: 'hidden'
        }, {
            fieldLabel: '責任中心',
            name: 'DEPT',
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
            readOnly: true,
            submitValue: true
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
            fieldLabel: '狀態',//紅底顯示
            name: 'STATUS_TEXT',
            readOnly: true,
            fieldCls: 'required'
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("EXTYPE").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>異動類別不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>異動類別不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("QTY").getValue()) < 1) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>異動數量需大於等於1</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>異動數量需大於等於11</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("STATUS").getValue()) == "") {
                    Ext.Msg.alert('提醒', "<span style='color:red'>狀態不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>狀態不可為空</span>，請重新輸入。");
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
            var r = Ext.create('WEBAPP.model.PH_PUT_D'); //Scripts / app / model / PH_PUT_D.js
            T2Form.loadRecord(r);

            f2.findField("AGEN_NO").setValue(T1Form.getForm().findField("AGEN_NO").getValue());
            f2.findField("MMCODE").setValue(T1Form.getForm().findField("MMCODE").getValue());
            f2.findField("DEPT").setValue(T1Form.getForm().findField("DEPT").getValue());
            u = f2.findField("TXTDAY");
            u.setReadOnly(false);
            f2.findField("QTY").setReadOnly(false);
            f2.findField("STATUS").setValue("A");

            f2.findField("EXTYPE").setValue("20");
            setCmpShowCondition_D(true, false, false, true, false, true, false, true, false);
        }
        else {

            u = f2.findField('EXTYPE');
            msglabel("<span style='color:red'>注意! 已存在資料不能更改[異動數量],請新增加一筆資料由系統加減[同意寄放量]</span>");
            setCmpShowCondition_D(false, false, true, true, false, false, true, true, false);
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
        }
        ],
        columns: [{
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
                T2Rec = index;
                T2LastRec = record;
                setFormT2a();
                if (T2LastRec) {
                    msglabel("");
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
            var f = T2Form.getForm();
            var aa = f.findField('SEQ').getValue();

            f.findField('x').setValue('U');
            var u = f.findField('AGEN_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            if (T1Form.getForm().findField("STATUS").getValue() == "D" ||
                T2Form.getForm().findField("STATUS").getValue() == "B") {
                T2Grid.down('#edit').setDisabled(T2Rec === 1);
                // T2Grid.down('#delete').setDisabled(T2Rec === 1);
            }
            else {
                T2Grid.down('#edit').setDisabled(T2Rec === 0);
                //T2Grid.down('#delete').setDisabled(T2Rec === 0);
            }
        }
        else {
            T2Form.getForm().reset();
            T2Grid.down('#edit').setDisabled(T2Rec === 0);
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
                    setCmpShowCondition_D(false, false, true, false, true, false, true, false, true);
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T2Store.insert(0, r);
                            if (f2.findField("EXTYPE").getValue() == "20") {
                                var T1_AGEN_NO = T1Form.getForm().findField("AGEN_NO").getValue();
                                var T1_MMCODE = T1Form.getForm().findField("MMCODE").getValue();
                                var T1_DEPT = T1Form.getForm().findField("DEPT").getValue();
                                var T1_QTY = T1Form.getForm().findField("QTY").getValue();
                                for (var i = 0; i < T1Grid.getStore().data.items.length; i++) {
                                    if ((T1Grid.getStore().data.items[i].data.AGEN_NO == T1_AGEN_NO) &&
                                        (T1Grid.getStore().data.items[i].data.MMCODE == T1_MMCODE) &&
                                        (T1Grid.getStore().data.items[i].data.DEPT == T1_DEPT)) {
                                        T1Grid.getStore().data.items[i].data.QTY = T1_QTY - f2.findField("QTY").getValue();
                                        T1Form.getForm().findField("QTY").setValue(T1_QTY - f2.findField("QTY").getValue());
                                    }
                                }

                            }
                            msglabel(" 資料新增成功");
                            r.commit();
                            T1Form.getForm().getRecord().commit();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            msglabel(" 資料修改成功");
                            r.commit();
                            break;
                        case "D":
                            T2Store.remove(r);
                            msglabel(" 資料刪除成功");
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
        setCmpShowCondition_D(false, false, true, false, true, false, true, false, true);
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
            width: 350,
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
    function setCmpShowCondition(MMCODE, MMCODE_DISPLAY, MMCODE_TEXT, DEPT, DEPT_NAME_TEXT, DEPT_NAME_DISPLAY, QTY, QTY_DISPLAY, STATUS, STATUS_TEXT) {
        var f = T1Form.getForm();
        MMCODE ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        MMCODE_DISPLAY ? showComponent(f, "MMCODE_DISPLAY") : hideComponent(f, "MMCODE_DISPLAY");
        MMCODE_TEXT ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
        DEPT ? showComponent(f, "DEPT") : hideComponent(f, "DEPT");
        DEPT_NAME_TEXT ? showComponent(f, "DEPT_NAME_TEXT") : hideComponent(f, "DEPT_NAME_TEXT");
        DEPT_NAME_DISPLAY ? showComponent(f, "DEPT_NAME_DISPLAY") : hideComponent(f, "DEPT_NAME_DISPLAY");
        QTY ? showComponent(f, "QTY") : hideComponent(f, "QTY");
        QTY_DISPLAY ? showComponent(f, "QTY_DISPLAY") : hideComponent(f, "QTY_DISPLAY");
        STATUS ? showComponent(f, "STATUS") : hideComponent(f, "STATUS");
        STATUS_TEXT ? showComponent(f, "STATUS_TEXT") : hideComponent(f, "STATUS_TEXT");
    }
    function setCmpShowCondition_D(TXTDAY, TXTDAY_TEXT, TXTDAY_DISPLAY, EXTYPE, EXTYPE_TEXT, QTY, QTY_DISPLAY, STATUS, STATUS_TEXT) {
        var f = T2Form.getForm();
        TXTDAY ? showComponent(f, "TXTDAY") : hideComponent(f, "TXTDAY");
        TXTDAY_TEXT ? showComponent(f, "TXTDAY_TEXT") : hideComponent(f, "TXTDAY_TEXT");
        TXTDAY_DISPLAY ? showComponent(f, "TXTDAY_DISPLAY") : hideComponent(f, "TXTDAY_DISPLAY");
        EXTYPE ? showComponent(f, "EXTYPE") : hideComponent(f, "EXTYPE");
        EXTYPE_TEXT ? showComponent(f, "EXTYPE_TEXT") : hideComponent(f, "EXTYPE_TEXT");
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

    setCmpShowCondition(false, true, false, false, false, true, false, true, false, true);
    setCmpShowCondition_D(false, false, true, false, true, false, true, false, true);
    Ext.getCmp("mmcodeComboSet_i").getComponent('btnMmcode_i').hide();
    T1QueryForm.getForm().findField('P0').focus();
    SetAGEN_NO_Q();
    SetINIDNAME();
    SetSTATUS_M();
    SetEXTYPE();
    SetSTATUS_D();
});
