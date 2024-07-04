Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0027/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T2Set = ''; // 新增/修改/刪除
    var T1Name = "廠商換貨(金額)作業";
    var APP_ID_NAME = '';

    var T1Rec = 0;
    var T1LastRec = null;

    var start_date = '';
    var end_date = '';
    var wh_no = '';

    var GetWH_NO = '../../../api/AA0027/GetWH_NO';
    var GetAPPID_NAME = '../../../api/AA0027/GetAPPID_NAME';
    var GetDocno = '../../../api/AA0027/GetDocno';

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
                return ""
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
                        ));
            }
        }
    });

    function GetDate() {
        end_date = new Date();
        end_date = Ext.Date.format(end_date, "Ymd") - 19110000;

        start_date = new Date();
        start_date.setDate(start_date.getDate() - 180);//7天前的日期
        var y = start_date.getFullYear();
        var m = (start_date.getMonth() + 1) < 10 ? "0" + (start_date.getMonth() + 1) : (start_date.getMonth() + 1);
        var d = start_date.getDate() < 10 ? "0" + start_date.getDate() : start_date.getDate();
        start_date = y + "-" + m + "-" + d;
    }

    var WH_NO_Store = Ext.create('Ext.data.Store', {  //調帳庫別的store
        fields: ['VALUE', 'TEXT']
    });

    function SetAPPID_NAME() { //取得建立人員
        Ext.Ajax.request({
            url: GetAPPID_NAME,
            method: reqVal_g,
            success: function (response) {
                APP_ID_NAME = response.responseText.replace(new RegExp('"', 'g'), '');
            },
            failure: function (response, options) {
            }
        });
    }

    function SetAPPID() { //取得建立人員
        Ext.Ajax.request({
            url: GetAPPID,
            method: reqVal_g,
            success: function (response) {
                APP_ID = response.responseText.replace(new RegExp('"', 'g'), '');
            },
            failure: function (response, options) {
            }
        });
    }

    function SetDocno() { //取得調帳單號
        Ext.Ajax.request({
            url: GetDocno,
            method: reqVal_g,
            success: function (response) {
                T1Form.getForm().findField("DOCNO").setValue(response.responseText.replace(new RegExp('"', 'g'), ''));
            },
            failure: function (response, options) {
            }
        });
    }

    // 查詢欄位
    var T1QueryForm = Ext.widget({
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
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '調帳日期區間',
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
                },
                {
                    xtype: 'displayfield',
                    fieldLabel: '調帳庫別',
                    value: 'PH1S 藥庫',
                    name: 'P2',
                    id: 'P2',
                },
                //{
                //    xtype: 'combo',
                //    fieldLabel: '調帳庫別',
                //    name: 'P2',
                //    id: 'P2',
                //    store: WH_NO_Store,
                //    queryMode: 'local',
                //    displayField: 'TEXT',
                //    valueField: 'VALUE',
                //    fieldCls: 'required',
                //    allowBlank: false,
                //    blankText: "請輸入調帳庫別",
                //    autoSelect: true,
                //    forceSelection: true,
                //    labelWidth: 80,
                //    width: 280,
                //    anyMatch: true
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        if (T1QueryForm.getForm().findField('P2').getValue() == null ||
                            T1QueryForm.getForm().findField('P2').getValue().replace(new RegExp(" ", "g"), "") == '') {
                            Ext.Msg.alert('提醒', "<span style='color:red'>調帳庫別不可為空</span>，請重新輸入。");
                            msglabel(" <span style='color:red'>調帳庫別不可為空</span>，請重新輸入。");
                        }
                        else {
                            T1Load();
                        }
                        msglabel("");
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        T1QueryForm.getForm().findField('P0').setValue(start_date);
                        T1QueryForm.getForm().findField('P1').setValue(end_date);
                        T1QueryForm.getForm().findField('P2').setValue(wh_no);
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0027M', { // 定義於/Scripts/app/store/AA/AA0027M.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').getValue(),
                    p2: 'PH1S'
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
                    T1Set = '../../../api/AA0027/CreateM';
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '../../../api/AA0027/UpdateM';
                    setFormT1("U", '修改');
                }
            },
            {
                itemId: 'back', text: '撤銷', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('撤銷', '是否確定撤銷？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0027/BackM';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'runM', text: '執行過帳', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('執行過帳', '是否確定執行過帳？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0027/RunM';
                            T1Form.getForm().findField('x').setValue('R');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'print', id:'print', text: '列印', handler: function () {
                    showWin3();
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
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                fieldLabel: '狀態代碼',
                name: 'FLOWID',
                xtype: 'hidden'
            }, {
                xtype: 'displayfield',
                fieldLabel: '調帳單號', //白底顯示
                name: 'DOCNO',
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '狀態',//白底顯示
                name: 'FLOWID_NAME',
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'datefield',
                fieldLabel: '調帳日期',
                name: 'APPTIME',
                id: 'APPTIME',
                enforceMaxLength: true,
                allowBlank: true, // 欄位是否為必填
                //format: 'X/m/d',
                renderer: function (value, meta, record) {
                    return Ext.util.Format.date(value, 'X/m/d');
                }
            }, {
                fieldLabel: '調帳日期', //白框顯示
                name: 'APPTIME_TEXT',
                readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '調帳庫別',
                name: 'FRWH',
                id: 'FRWH',
                value: 'PH1S',
                submitValue:true
            },
            //{
            //    xtype: 'combo',
            //    fieldLabel: '調帳庫別',
            //    name: 'FRWH',
            //    id: 'FRWH',
            //    store: WH_NO_Store,
            //    queryMode: 'local',
            //    displayField: 'TEXT',
            //    valueField: 'VALUE',
            //    fieldCls: 'required',
            //    forceSelection: true,
            //    allowBlank: false,
            //    blankText: "請輸入調帳庫別",
            //    autoSelect: true,
            //    anyMatch: true
            //},
            {
                fieldLabel: '調帳庫別',//紅底顯示
                name: 'FRWH_NAME',
                readOnly: true,
                fieldCls: 'required'
            },
            {
                fieldLabel: '備註', //白框顯示
                name: 'APPLY_NOTE',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '建立人員',//白底顯示
                name: 'APP_ID_NAME',
                value: APP_ID_NAME,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '最後異動日期',//白底顯示
                name: 'UPDATE_TIME',
                readOnly: true
            }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if ((T1Form.getForm().findField("FRWH").getValue()) == "" ||
                    (T1Form.getForm().findField("FRWH").getValue()) == null) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>調帳庫別不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>調帳庫別不可為空</span>，請重新輸入。");
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

        dockedItems: [{
            items: [T1QueryForm]
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
            text: "調帳單號",
            dataIndex: 'DOCNO',
            width: 120
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_NAME',
            width: 130
        }, {
            text: "調帳日期",
            dataIndex: 'APPTIME',
            width: 90
        }, {
            text: "調帳庫別",
            dataIndex: 'FRWH_NAME',
            width: 180
        }, {
            text: "備註",
            dataIndex: 'APPLY_NOTE',
            width: 220
        }, {
            text: "建立人員",
            dataIndex: 'APP_ID_NAME',
            width: 100
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
                setFormT1a();
                if (T1LastRec) {
                    viewport.down('#form').expand();
                    msglabel("");
                }
            },
            itemclick: function (self, record, item, index, e, eOpts) {
                T1Rec = index;
                T1LastRec = record;
                setFormT1a();
                if (T1LastRec) {
                    viewport.down('#form').expand();
                    msglabel("");
                }
            }
        }
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + "調帳單");
        viewport.down('#form').expand();
        T1Form.setVisible(true);
        T2Form.setVisible(false);

        var f = T1Form.getForm();
        msglabel("");

        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.AA.AA0027M'); // /Scripts/app/model/AA/AA0027M.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            SetDocno();
            f.findField('APPTIME').setValue(new Date());
            f.findField('APP_ID_NAME').setValue(APP_ID_NAME);
            f.findField('FRWH').setValue('PH1S');
            f.findField('FRWH_NAME').setValue('PH1S');
        }
        setCmpShowCondition(true, false, true, false, true);

        f.findField('x').setValue(x);
        u = f.findField('APPTIME');
        f.findField('FLOWID_NAME').setValue('換貨申請中');
        f.findField('APPTIME').setReadOnly(false);
        f.findField('FRWH').setReadOnly(false);
        f.findField('APPLY_NOTE').setReadOnly(false);
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
                    setCmpShowCondition(false, true, false, true, true);
                    //T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var frwhvalue = T1Form.getForm().findField("FRWH").getValue();
                    var start_datevalue = T1Form.getForm().findField("APPTIME").getValue();
                    var end_datevalue = T1Form.getForm().findField("APPTIME").getValue();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel(" 資料新增成功");
                            T1QueryForm.getForm().findField("P0").setValue(start_datevalue);
                            T1QueryForm.getForm().findField("P1").setValue(end_datevalue);
                            T1QueryForm.getForm().findField("P2").setValue(frwhvalue);
                            break;
                        case "U":
                            msglabel(" 資料修改成功");
                            break;
                        case "D":
                            //T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            msglabel(" 資料撤銷成功");
                            //r.commit();
                            break;
                        case "R":
                            //T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            Ext.Msg.alert('提醒', '調帳單號' + T1Form.getForm().findField('DOCNO').getValue() + ' 已執行過帳');
                            msglabel('調帳單號' + T1Form.getForm().findField('DOCNO').getValue() + ' 已執行過帳');
                            //r.commit();
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
                            if ((action.result.msg == null) || (action.result.msg == '')) {
                                Ext.Msg.alert('失敗', "執行失敗，發生例外錯誤 ");
                                msglabel("執行失敗，發生例外錯誤 ");
                            }
                            else {
                                Ext.Msg.alert('失敗', action.result.msg);
                                msglabel(" " + action.result.msg);
                            }
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
            f.findField('x').setValue('U');
            viewport.down('#form').expand();

            if (T1Form.getForm().findField("FLOWID").getValue() == '1001') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#back').setDisabled(false);
                //T1Grid.down('#runM').setDisabled(false);
                T2Grid.down('#add').setDisabled(false);
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
            else if (T1Form.getForm().findField("FLOWID").getValue() == 'X') {
                T1Grid.down('#edit').setDisabled(false);
                T1Grid.down('#back').setDisabled(true);
                T1Grid.down('#runM').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
            else {
                T1Grid.down('#edit').setDisabled(true);
                T1Grid.down('#back').setDisabled(true);
                T1Grid.down('#runM').setDisabled(true);
                T2Grid.down('#add').setDisabled(true);
                T2Grid.down('#edit').setDisabled(true);
                T2Grid.down('#delete').setDisabled(true);
            }
        }
        else {
            T1Form.getForm().reset();
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#back').setDisabled(true);
            T1Grid.down('#runM').setDisabled(true);
            T2Grid.down('#add').setDisabled(true);
            T2Grid.down('#edit').setDisabled(true);
            T2Grid.down('#delete').setDisabled(true);
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

        if (T1Form.getForm().findField("FLOWID").getValue() == '1001') {
            T1Grid.down('#edit').setDisabled(false);
            T1Grid.down('#back').setDisabled(false);
        }
        else if (T1Form.getForm().findField("FLOWID").getValue() == 'X') {
            T1Grid.down('#edit').setDisabled(false);
            T1Grid.down('#back').setDisabled(true);
        }
        else {
            T1Grid.down('#edit').setDisabled(true);
            T1Grid.down('#back').setDisabled(true);
        }

        viewport.down('#form').setTitle('瀏覽');
        viewport.down('#form').setCollapsed("true");
        T2Grid.down('#add').setDisabled(T1Rec === 1);
        setCmpShowCondition(false, true, false, true, true);
        setFormT1a();
    }


    ///////////////////////
    ///////  DETAIL  //////
    ///////////////////////

    var T2Rec = 0;
    var T2LastRec = null;

    var GetLOT_NO = '../../../api/AA0027/GetLOT_NO';
    var GetEXP_DATE = '../../../api/AA0027/GetEXP_DATE';
    var GetINV_QTY = '../../../api/AA0027/GetINV_QTY';
    var GetM_DISCPERC = '../../../api/AA0027/GetM_DISCPERC';
    var GetM_CONTPRICE = '../../../api/AA0027/GetM_CONTPRICE';

    var DISCPERC = '';
    var CONTPRICE = '';
    var NEW_APVQTY = '';

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        id: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        autoSelect: true,
        anyMatch: true,
        fieldStyle: 'text-transform:uppercase',
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0027/GetMMCodeCombo',
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('FRWH').getValue()
            };
        },

        //查詢完會回傳的欄位
        //extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("AGEN_NAME").setValue(r.get('AGEN_NAMEC'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                    Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ' + r.get('M_DISCPERC'));
                    Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + r.get('M_CONTPRICE'));
                    DISCPERC = r.get('M_DISCPERC');
                    CONTPRICE = r.get('M_CONTPRICE');
                    SetLOT_NO(r.get('MMCODE'));
                }
                else {
                    f.findField("MMCODE").setValue("");
                    f.findField("MMNAME_C").setValue("");
                    f.findField("MMNAME_E").setValue("");
                    f.findField("AGEN_NAMEC").setValue("");
                    f.findField("BASE_UNIT").setValue("");
                    Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
                    Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
                    DISCPERC = '';
                    CONTPRICE = '';
                }
                f.findField("LOT_NO").setValue("");
                f.findField("LOT_NO").clearInvalid();
                f.findField("EXP_DATE").setValue("");
                f.findField("INV_QTY").setValue("");
                f.findField("APVQTY").setValue("");
                f.findField("APVQTY").clearInvalid();
            }
        }
    });

    function setMmcode(args) {
        if (args.MMCODE !== '') {
            T2Form.getForm().findField("MMCODE").setValue(args.MMCODE);
            T2Form.getForm().findField("MMCODE").clearInvalid();
            T2Form.getForm().findField("MMNAME_C").setValue(args.MMNAME_C);
            T2Form.getForm().findField("MMNAME_E").setValue(args.MMNAME_E);
            T2Form.getForm().findField("AGEN_NAME").setValue(args.AGEN_NAMEC);
            T2Form.getForm().findField("BASE_UNIT").setValue(args.BASE_UNIT);
            Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ' + args.M_DISCPERC);
            Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + args.M_CONTPRICE);
            DISCPERC = args.M_DISCPERC;
            CONTPRICE = args.M_CONTPRICE;
            SetLOT_NO(args.MMCODE);
        }
        else {
            T2Form.getForm().findField("MMCODE").setValue("");
            T2Form.getForm().findField("MMNAME_C").setValue("");
            T2Form.getForm().findField("MMNAME_E").setValue("");
            T2Form.getForm().findField("AGEN_NAMEC").setValue("");
            T2Form.getForm().findField("BASE_UNIT").setValue("");
            Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
            Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
            DISCPERC = '';
            CONTPRICE = '';
        }
        T2Form.getForm().findField("LOT_NO").setValue("");
        T2Form.getForm().findField("LOT_NO").clearInvalid();
        T2Form.getForm().findField("EXP_DATE").setValue("");
        T2Form.getForm().findField("INV_QTY").setValue("");
        T2Form.getForm().findField("APVQTY").setValue("");
        T2Form.getForm().findField("APVQTY").clearInvalid();
    }

    var LOT_NO_Store = Ext.create('Ext.data.Store', {  //調帳庫別的store
        fields: ['LOT_NO', 'EXP_DATE', 'INV_QTY']
    });

    function SetLOT_NO(MMCODE) { //建立批號的下拉式選單
        var qty_temp = 0;
        Ext.Ajax.request({
            url: GetLOT_NO,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: MMCODE
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var lot_nos = data.etts;
                    LOT_NO_Store.removeAll();
                    if (lot_nos.length > 0) {
                        for (var i = 0; i < lot_nos.length; i++) {
                            LOT_NO_Store.add({ LOT_NO: lot_nos[i].LOT_NO, EXP_DATE: lot_nos[i].EXP_DATE, INV_QTY: lot_nos[i].INV_QTY });
                            T2Form.getForm().findField("LOT_NO").setValue(lot_nos[0].LOT_NO);
                            T2Form.getForm().findField("EXP_DATE").setValue(lot_nos[0].EXP_DATE);
                            T2Form.getForm().findField("INV_QTY").setValue(lot_nos[0].INV_QTY);
                            qty_temp = lot_nos[0].INV_QTY;
                        }
                        T2Form.getForm().findField("APVQTY").setMaxValue(T2Form.getForm().findField("INV_QTY").getValue());
                        if (T2Form.getForm().findField("APVQTY").getValue() == null) {
                            T2Form.getForm().findField("APVQTY").setValue(qty_temp);
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetEXP_DATE() { //取得效期
        Ext.Ajax.request({
            url: GetEXP_DATE,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue(),
                LOT_NO: T2Form.getForm().findField("LOT_NO").getValue()
            },
            success: function (response) {
                T2Form.getForm().findField("EXP_DATE").setValue(response.responseText);
            },
            failure: function (response, options) {
            }
        });
    }

    function SetINV_QTY() { //取得效期數量
        Ext.Ajax.request({
            url: GetINV_QTY,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue(),
                LOT_NO: T2Form.getForm().findField("LOT_NO").getValue()
            },
            success: function (response) {
                T2Form.getForm().findField("INV_QTY").setValue(response.responseText);
            },
            failure: function (response, options) {
            }
        });
    }

    function SetM_DISCPERC() { //取得進貨單價
        Ext.Ajax.request({
            url: GetM_DISCPERC,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                DISCPERC = response.responseText.replace(new RegExp('"', 'g'), '');
                Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ' + DISCPERC);
                NEW_APVQTY = T2Form.getForm().findField("APVQTY").getValue();
                if (T2Form.getForm().findField('IN_PRICE').getValue() != '' &&
                    T2Form.getForm().findField('IN_PRICE').getValue() != null) {
                    T2Form.getForm().findField("IN_PRICE_1").setValue(true);
                    T2Form.getForm().findField("CONTPRICE_1").setValue(false);
                    T2Form.getForm().findField("C_AMT").setValue(Math.ceil(NEW_APVQTY * DISCPERC ));
                }
                else if (T2Form.getForm().findField('CONTPRICE').getValue() != '' &&
                    T2Form.getForm().findField('CONTPRICE').getValue() != null) {
                    T2Form.getForm().findField("IN_PRICE_1").setValue(false);
                    T2Form.getForm().findField("CONTPRICE_1").setValue(true);
                    T2Form.getForm().findField("C_AMT").setValue(Math.ceil(NEW_APVQTY * CONTPRICE ));
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetM_CONTPRICE() { //取得合約單價
        Ext.Ajax.request({
            url: GetM_CONTPRICE,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: T2Form.getForm().findField("MMCODE").getValue()
            },
            success: function (response) {
                CONTPRICE = response.responseText.replace(new RegExp('"', 'g'), '');
                Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ' + CONTPRICE);
            },
            failure: function (response, options) {
            }
        });
    }

    var T2Store = Ext.create('WEBAPP.store.AA.AA0027D', { // 定義於/Scripts/app/store/AA/AA0027D.js
        listeners: {
            //載入前
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1Form.getForm().findField("FRWH").getValue(),
                    p1: T1Form.getForm().findField("DOCNO").getValue()
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
                    T2Set = '../../../api/AA0027/CreateD';
                    T2Form.getForm().findField("C_TYPE").setValue({C_TYPE: 1});
                    setFormT2('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T2Set = '../../../api/AA0027/UpdateD';
                    setFormT2("U", '修改');
                }
            },
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T2Set = '../../../api/AA0027/DeleteD';
                            T2Form.getForm().findField('x').setValue('D');
                            T2Submit_Del();
                        }
                    });
                }
            }
        ]
    });

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
            width: 70
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 200
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 250
        }, {
            text: "廠商",
            dataIndex: 'AGEN_NAME',
            width: 170
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 80
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 70
        }, {
            text: "退貨量",
            dataIndex: 'APVQTY',
            width: 80,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50
        }, {
            text: "進貨/合約",
            dataIndex: 'C_TYPE_NAME',
            width: 80
        }, {
            text: "進貨單價",
            dataIndex: 'IN_PRICE',
            width: 70,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "合約單價",
            dataIndex: 'CONTPRICE',
            width: 70,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "換貨金額",
            dataIndex: 'C_AMT',
            width: 80,
            xtype: 'numbercolumn',
            align: 'right',
            style: 'text-align:left',
            format: '0.00'
        }, {
            text: "備註",
            dataIndex: 'ITEM_NOTE',
            width: 100
        }, {
            header: "",
            flex: 1
        }
        ],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    if (T2Store.getCount() === 0) {
                        T1Tool.down('#runM').setDisabled(true);
                    }
                    else if ((T2Store.getCount() != 0) && (T1Form.getForm().findField('FLOWID').getValue() == '1001')) {
                        T1Tool.down('#runM').setDisabled(false);
                    }
                }
            }
        },
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
            itemclick: function (self, record, item, index, e, eOpts) {
                //T2Rec = records.length;
                T2LastRec = record;
                setFormT2a();
                if (T2LastRec) {
                    msglabel("");
                }
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        hidden: true,
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T2b',
        autoScroll: true,
        title: '',
        bodyPadding: '10 0 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 65,
            width: 250
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
                fieldLabel: '調帳單號',
                name: 'DOCNO',
                xtype: 'hidden'
            }, {
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            }, {
                fieldLabel: '單價',
                name: 'C_UP',
                xtype: 'hidden'
            }, {
                fieldLabel: '調帳庫別',
                name: 'FRWH',
                xtype: 'hidden'
            }, {
                xtype: 'container',
                layout: 'hbox',
                //padding: '0 0 7 0',
                id: 'mmcodeComboSet',
                items: [
                    mmCodeCombo,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            popMmcodeForm(viewport, '/api/AA0027/GetMmcode',
                                {
                                    MMCODE: f.findField("MMCODE").getValue(),
                                    FRWH: T1Form.getForm().findField("FRWH").getValue(),
                                }, setMmcode);
                        }
                    }
                ]
            }, {
                fieldLabel: '院內碼',//紅底顯示
                name: 'MMCODE_TEXT',
                readOnly: true,
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '中文品名', //白底顯示
                name: 'MMNAME_C',
                readOnly: true,
                width: '100%'
            }, {
                xtype: 'displayfield',
                fieldLabel: '英文品名', //白底顯示
                name: 'MMNAME_E',
                readOnly: true,
                width: '100%'
            }, {
                xtype: 'displayfield',
                fieldLabel: '廠商', //白底顯示
                name: 'AGEN_NAME',
                readOnly: true,
                width: '100%'
            }, {
                xtype: 'combo',
                fieldLabel: '批號',
                name: 'LOT_NO',
                id: 'LOT_NO',
                store: LOT_NO_Store,
                queryMode: 'local',
                displayField: 'LOT_NO',
                valueField: 'LOT_NO',
                autoSelect: true,
                anyMatch: true,
                fieldCls: 'required',
                forceSelection: true,
                allowBlank: false,
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{LOT_NO}&nbsp;</div></tpl>',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {
                        T2Form.getForm().findField("EXP_DATE").setValue(newValue.data.EXP_DATE);
                        T2Form.getForm().findField("INV_QTY").setValue(newValue.data.INV_QTY);
                        T2Form.getForm().findField("APVQTY").setMaxValue(T2Form.getForm().findField("INV_QTY").getValue());
                        T2Form.getForm().findField("APVQTY").setValue(newValue.data.INV_QTY);
                    }
                }
            }, {
                fieldLabel: '批號', //白底顯示
                name: 'LOT_NO_TEXT',
                readOnly: true,
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '效期', //白底顯示
                name: 'EXP_DATE',
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '效期數量', //白底顯示
                name: 'INV_QTY',
                readOnly: true
            }, {
                xtype: "numberfield",
                fieldLabel: '退貨量',
                name: 'APVQTY',
                minValue: 1,
                enforceMaxLength: true,
                maxLength: 15,
                readOnly: true,
                fieldCls: 'required',
                allowBlank: false,
                hideTrigger: true,
                decimalPrecision: 2,
                allowDecimals: true,
                forcePrecision: true,
                listeners: {
                    change: function (field, newValue, oldValue) {
                        NEW_APVQTY = newValue;
                        if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == true) &&
                            (T2Form.getForm().findField("CONTPRICE_1").getValue() == false)) {
                            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(DISCPERC * NEW_APVQTY ));
                        } if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == false) &&
                            (T2Form.getForm().findField("CONTPRICE_1").getValue() == true)) {
                            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(CONTPRICE * NEW_APVQTY));
                        }
                    }
                }
            }, {
                fieldLabel: '退貨量', //紅底顯示
                name: 'APVQTY_TEXT',
                readOnly: true,
                fieldCls: 'required'
            }, {
                xtype: 'displayfield',
                fieldLabel: '單位', //白底顯示
                name: 'BASE_UNIT',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '進貨單價', //白底顯示
                name: 'IN_PRICE',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '合約單價', //白底顯示
                name: 'CONTPRICE',
                readOnly: true
            },
            {
                xtype: 'radiogroup',
                name: 'C_TYPE',
                id: 'C_TYPE',
                fieldLabel: '單價',
                labelWidth: 65,
                width: 200,
                columns: 1,
                fieldCls: 'required',
                allowBlank: false,
                items: [
                    { boxLabel: '進貨單價', id: 'IN_PRICE_1', inputValue: '1', width: 150, checked: true },
                    { boxLabel: '合約單價', id: 'CONTPRICE_1', inputValue: '2', width: 150 },
                    { boxLabel: '其他', id: 'OTHER', inputValue: '3', width: 150, hidden: true }
                ],
                listeners: {
                    change: function (field, newValue, oldValue) {
                        
                        if (newValue.C_TYPE == '1') {
                            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(DISCPERC * NEW_APVQTY ));
                            T2Form.getForm().findField("C_UP").setValue(DISCPERC);
                        }
                        else if (newValue.C_TYPE == '2') {
                            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(CONTPRICE * NEW_APVQTY));
                            T2Form.getForm().findField("C_UP").setValue(CONTPRICE);
                        }
                    }
                }
            }, {
                xtype: 'displayfield',
                fieldLabel: '換貨金額', //白底顯示
                name: 'C_AMT',
                readOnly: true,
                submitValue: true
            }, {
                fieldLabel: '項次備註', //白底顯示
                name: 'ITEM_NOTE',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '建立人員', //白底顯示
                name: 'UPDATE_USER',
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '建立日期', //白底顯示
                name: 'UPDATE_DATE',
                value: Ext.Date.format(new Date(), "Ymd") - 19110000,
                readOnly: true
            }]
        }],

        buttons: [{
            itemId: 'T2Submit', text: '儲存', hidden: true, handler: function () {
                if ((T2Form.getForm().findField("MMCODE").getValue()) == "" ||
                    (T2Form.getForm().findField("MMCODE").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>院內碼不可為空</span>，請重新輸入。");
                }
                else if ((T2Form.getForm().findField("LOT_NO").getValue()) == "" ||
                    (T2Form.getForm().findField("LOT_NO").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>批號不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>批號不可為空</span>，請重新輸入。");
                } else if ((T2Form.getForm().findField("APVQTY").getValue()) == "" ||
                    (T2Form.getForm().findField("APVQTY").getValue() == null)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>退貨量不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>退貨量不可為空</span>，請重新輸入。");
                } else if ((T2Form.getForm().findField("IN_PRICE_1").getValue() == false) &&
                    (T2Form.getForm().findField("CONTPRICE_1").getValue() == false)) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>單價需二擇一</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>單價需二擇一</span>，請重新輸入。");
                }
                else if (T2Form.getForm().findField("C_TYPE").getValue().C_TYPE == '3') {
                    Ext.Msg.alert('提醒', "<span style='color:red'>單價需二擇一</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>單價需二擇一</span>，請重新輸入。");
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
        viewport.down('#form').setTitle(t + '調帳品項');
        viewport.down('#form').expand();
        T1Form.setVisible(false);
        T2Form.setVisible(true);
        var f2 = T2Form.getForm();
        msglabel("");
        if (x === "I") {
            isNew = true;
            f2.reset();
            var r = Ext.create('WEBAPP.model.AA.AA0027D'); //Scripts / app / model / AA/AA0027D.js
            T2Form.loadRecord(r);
            u = f2.findField("MMCODE");
            f2.findField("UPDATE_USER").setValue(APP_ID_NAME);
            f2.findField("DOCNO").setValue(T1Form.getForm().findField("DOCNO").getValue());
            f2.findField("FRWH").setValue(T1Form.getForm().findField("FRWH").getValue());
            Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
            Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');

            setCmpShowCondition_D(true, false, true, false, true, false, false, false);
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').show();
            Ext.getCmp("C_TYPE").show();
        }
        else {
            u = f2.findField('MMCODE');
            setCmpShowCondition_D(true, false, true, false, true, false, false, false);
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').show();
            Ext.getCmp("C_TYPE").show();
            SetM_CONTPRICE();
            SetM_DISCPERC();
            SetLOT_NO(T2Form.getForm().findField('MMCODE').getValue());
            T2Form.getForm().findField("APVQTY").setMaxValue(T2Form.getForm().findField("INV_QTY").getValue());
            T2Form.getForm().findField("APVQTY").setValue(T2Form.getForm().findField("APVQTY_TEXT").getValue());
        }

        f2.findField('x').setValue(x);
        f2.findField('MMCODE').setReadOnly(false);
        f2.findField('MMCODE').clearInvalid();
        f2.findField('LOT_NO').setReadOnly(false);
        f2.findField('LOT_NO').clearInvalid();
        f2.findField('APVQTY').setReadOnly(false);
        f2.findField('APVQTY').clearInvalid();
        f2.findField('C_TYPE').setReadOnly(false);
        f2.findField('ITEM_NOTE').setReadOnly(false);
        T2Form.down('#T2Cancel').setVisible(true);
        T2Form.down('#T2Submit').setVisible(true);
        u.focus();
    }

    //Detail按儲存之後
    function T2Submit() {
        var f = T2Form.getForm();
        if (T2Form.getForm().findField("C_TYPE").getValue().C_TYPE == '1') {
            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(DISCPERC * NEW_APVQTY));
            T2Form.getForm().findField("C_UP").setValue(DISCPERC);
        }
        else if (T2Form.getForm().findField("C_TYPE").getValue().C_TYPE == '2') {
            T2Form.getForm().findField("C_AMT").setValue(Math.ceil(CONTPRICE * NEW_APVQTY));
            T2Form.getForm().findField("C_UP").setValue(CONTPRICE);
        }
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            f.submit({
                url: T2Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition_D(false, true, false, true, false, true, true, true);
                    Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();
                    Ext.getCmp("C_TYPE").hide();
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

                            if (T2Form.getForm().findField("x").getValue() == 'I') {
                                T2Form.getForm().findField('SEQ').setValue('');
                                T2Form.getForm().findField('C_UP').setValue('');
                                T2Form.getForm().findField('MMCODE').setValue('');
                                T2Form.getForm().findField('MMCODE').clearInvalid();
                                T2Form.getForm().findField('MMNAME_C').setValue('');
                                T2Form.getForm().findField('MMNAME_E').setValue('');
                                T2Form.getForm().findField('AGEN_NAME').setValue('');
                                T2Form.getForm().findField('LOT_NO').setValue('');
                                T2Form.getForm().findField('LOT_NO').clearInvalid();
                                T2Form.getForm().findField('EXP_DATE').setValue('');
                                T2Form.getForm().findField('INV_QTY').setValue('');
                                T2Form.getForm().findField('APVQTY').setValue('');
                                T2Form.getForm().findField('APVQTY').clearInvalid();
                                T2Form.getForm().findField('BASE_UNIT').setValue('');
                                T2Form.getForm().findField('C_TYPE').setValue('');
                                Ext.getCmp("IN_PRICE_1").setBoxLabel('進貨單價: ');
                                Ext.getCmp("CONTPRICE_1").setBoxLabel('合約單價: ');
                            }
                            else if (T2Form.getForm().findField("x").getValue() == 'U') {
                                T2Cleanup();
                            }
                            break;
                    }
                }
            });
        }
    }

    //Detail按刪除之後
    function T2Submit_Del() {
        var f = T2Form.getForm();
        if (f.findField('DOCNO').getValue() != '' &&
            f.findField('DOCNO').getValue() != null &&
            f.findField('SEQ').getValue() != '' &&
            f.findField('SEQ').getValue() != null) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();

            Ext.Ajax.request({
                url: T2Set,
                params: {
                    DOCNO: f.findField('DOCNO').getValue(),
                    SEQ: f.findField('SEQ').getValue()
                },
                success: function (response) {
                    myMask.hide();
                    setCmpShowCondition_D(false, true, false, true, false, true, true, true);
                    Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();
                    Ext.getCmp("C_TYPE").hide();
                    msglabel(" 資料刪除成功");

                    T2Cleanup();
                    T2Load();
                },
                failure: function (response, action) {
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

    //點選detail項目後
    function setFormT2a() {
        if (T2LastRec) {
            isNew = false;
            T2Form.loadRecord(T2LastRec);
            var f = T2Form.getForm();
            viewport.down('#form').expand();

            f.findField('x').setValue('U');
            T2Grid.down('#edit').setDisabled(T1Form.getForm().findField("FLOWID").getValue() != '1001');
            T2Grid.down('#delete').setDisabled(T1Form.getForm().findField("FLOWID").getValue() != '1001');
            T2Form.getForm().findField("APVQTY").setMaxValue(T2Form.getForm().findField("INV_QTY").getValue());
        }
        else {
            T2Form.getForm().reset();
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
        LOT_NO_Store.removeAll();
        //viewport.down('#form').setTitle('瀏覽');
        //setFormT2a();
        T2Grid.down('#edit').setDisabled(true);
        T2Grid.down('#delete').setDisabled(true);
        setCmpShowCondition_D(false, true, false, true, false, true, true, true);
        Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();
        Ext.getCmp("C_TYPE").hide();
        viewport.down('#form').setCollapsed("true");
    }

    //#region 2020-08-18 新增: 列印
    var reportUrl = '/Report/A/AA0027.aspx';
    var T3Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 90
        },
        autoScroll: true,
        items: [
            {
                xtype: 'datefield',
                fieldLabel: '調帳日期區間',
                labelAlign: 'right',
                name: 'T3P0',
                id: 'T3P0',
                vtype: 'dateRange',
                dateRange: { end: 'T3P1' },
                padding: '0 4 0 4',
                labelWidth: 80,
                width: 170,
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelAlign: 'right',
                labelWidth: 10,
                name: 'T3P1',
                id: 'T3P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'T3P0' },
                padding: '0 4 0 4',
                width: 90,
                value: getToday()
            }
        ],
        buttons: [{
            itemId: 'T3print', text: '列印', handler: function () {
                showReport();
            }
        },
        {
            itemId: 'T3cancel', text: '取消', handler: hideWin3
        }
        ]
    });

    var win3;
    var winActWidth3 = 300;
    var winActHeight3 = 200;
    if (!win3) {
        win3 = Ext.widget('window', {
            title: '列印',
            closeAction: 'hide',
            width: winActWidth3,
            height: winActHeight3,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: T3Form,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth3 > 0) ? winActWidth3 : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight3 > 0) ? winActHeight3 : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth3 = width;
                    winActHeight3 = height;
                }
            }
        });
    }
    function showWin3() {
        if (win3.hidden) {
            win3.show();
        }
    }
    function hideWin3() {
        if (!win3.hidden) {
            win3.hide();
        }
    }
    function showReport() {

        var qstring = '?p0=' + T3Form.getForm().findField('T3P0').rawValue + '&p1=' + T3Form.getForm().findField('T3P1').rawValue;
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + qstring + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        hideWin3();
    }
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    //#endregion

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
            items: [T1Form, T2Form]
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

    //控制不可更改項目的顯示
    function setCmpShowCondition(APPTIME, APPTIME_TEXT, FRWH, FRWH_NAME, UPDATE_TIME) {
        var f = T1Form.getForm();
        
        APPTIME ? showComponent(f, "APPTIME") : hideComponent(f, "APPTIME");
        APPTIME_TEXT ? showComponent(f, "APPTIME_TEXT") : hideComponent(f, "APPTIME_TEXT");
        FRWH ? showComponent(f, "FRWH") : hideComponent(f, "FRWH");
        FRWH_NAME ? showComponent(f, "FRWH_NAME") : hideComponent(f, "FRWH_NAME");
        UPDATE_TIME ? showComponent(f, "UPDATE_TIME") : hideComponent(f, "UPDATE_TIME");
    }

    function setCmpShowCondition_D(MMCODE, MMCODE_TEXT, LOT_NO, LOT_NO_TEXT, APVQTY, APVQTY_TEXT, IN_PRICE, CONTPRICE) {
        var f = T2Form.getForm();
        MMCODE ? showComponent(f, "MMCODE") : hideComponent(f, "MMCODE");
        MMCODE_TEXT ? showComponent(f, "MMCODE_TEXT") : hideComponent(f, "MMCODE_TEXT");
        LOT_NO ? showComponent(f, "LOT_NO") : hideComponent(f, "LOT_NO");
        LOT_NO_TEXT ? showComponent(f, "LOT_NO_TEXT") : hideComponent(f, "LOT_NO_TEXT");
        APVQTY ? showComponent(f, "APVQTY") : hideComponent(f, "APVQTY");
        APVQTY_TEXT ? showComponent(f, "APVQTY_TEXT") : hideComponent(f, "APVQTY_TEXT");
        IN_PRICE ? showComponent(f, "IN_PRICE") : hideComponent(f, "IN_PRICE");
        CONTPRICE ? showComponent(f, "CONTPRICE") : hideComponent(f, "CONTPRICE");
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

    setCmpShowCondition(false, true, false, true, true);
    setCmpShowCondition_D(false, true, false, true, false, true, true, true);
    Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();
    Ext.getCmp("C_TYPE").hide();
    T1QueryForm.getForm().findField('P0').focus();
    GetDate();
    SetAPPID_NAME();
    T1QueryForm.getForm().findField('P0').setValue(start_date);
    T1QueryForm.getForm().findField('P1').setValue(end_date);
});
