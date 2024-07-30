Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common', 'WEBAPP.form.DocButton']);


Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name2 = "安全量、作業量";

    var arrP3 = ["0"];

    var T1RecLength = 0;
    var T1LastRec = null;

    var T1GetExcel = '../../../api/AA0048/Excel';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AA0048/GetWH_NOComboOne';  //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1', 'EXTRA2']
    });
    var wh_noStore_insert = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'EXTRA1', 'EXTRA2']
    });

    var st_isauto = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });



    function setComboData() {
        Ext.Ajax.request({
            url: WH_NOComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;

                    var combo_P0 = T1Query.getForm().findField('P0');
                    //if (tb_wh_no.length == 1) {

                    if (tb_wh_no.length > 0) {
                        //將第1筆資料代入
                        setTimeout(function () {
                            combo_P0.select(tb_wh_no[0].VALUE);
                            st_MatClass.load();
                            if (tb_wh_no[0].EXTRA1 == '0' && tb_wh_no[0].EXTRA2 >= '2') {
                                Ext.getCmp('edit').hide();
                                //Ext.getCmp('select_file').disable();
                            } else {
                                Ext.getCmp('edit').show();
                                //Ext.getCmp('select_file').enable();
                            }
                        }, 1000);
                    }
                    else {
                        combo_P0.setDisabled(false);
                    }
                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            wh_noStore.add({ VALUE: tb_wh_no[i].VALUE, TEXT: tb_wh_no[i].TEXT, EXTRA1: tb_wh_no[i].EXTRA1, EXTRA2: tb_wh_no[i].EXTRA2 });
                            if (tb_wh_no[i].EXTRA1 == '1') {
                                wh_noStore_insert.add({ VALUE: tb_wh_no[i].VALUE, TEXT: tb_wh_no[i].TEXT, EXTRA1: tb_wh_no[i].EXTRA1, EXTRA2: tb_wh_no[i].EXTRA2 });
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });


    }
    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        labelWidth: 60,
        width: 180,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0048/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //p0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0048/GetMatClassCombo',
            //getDefaultParams: function () { //查詢時Controller固定會收到的參數
            //    return {
            //        wh_no: T1Query.getForm().findField('P0').getValue()
            //    };
            //},
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    wh_no: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                //store.insert(0, { TEXT: '', VALUE: '' });
                if (records.length > 0) {
                    T1Query.getForm().findField('P2').setValue(records[0].data.VALUE);
                }
            }
        },
        autoLoad: true
    });

    var st_ctdmd = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0048/GetCtdmdCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    T1Query.getForm().findField('P3').setValue(records[0].data.VALUE);
                }
            }
        },
        autoLoad: true
    });
    var st_useadj_class = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0048/GetUseadjClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        }, listeners: {
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    // T1Query.getForm().findField('P3').setValue(records[0].data.VALUE);
                }
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
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
                    store: wh_noStore,
                    fieldLabel: '庫房代碼',
                    name: 'P0',
                    id: 'P0',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    multiSelect: true,
                    displayField: 'VALUE',
                    valueField: 'VALUE',
                    requiredFields: ['TEXT'],
                    labelWidth: 70,
                    width: 180,
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        "select": function (combobox, records, eOpts) {
                            st_MatClass.load();
                            //if (records.data.EXTRA1 == '0') {
                            //    Ext.getCmp('add').hide();
                            //    Ext.getCmp('delete').hide();
                            //} else {
                            //    Ext.getCmp('add').show();
                            //    Ext.getCmp('delete').show();
                            //}
                        },
                        blur: function (field, eOpts) {
                            chkWhNo();
                        }
                    },
                    padding: '0 4 0 4'
                },
                T1QuryMMCode,
                {
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    store: st_MatClass,
                    name: 'P2',
                    id: 'P2',
                    labelWidth: 70,
                    width: 180,
                    queryMode: 'local',
                    fieldCls: 'required',
                    allowBlank: false,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    listConfig:
                    {
                        width: 180
                    },
                    matchFieldWidth: false
                },
                {
                    xtype: 'combo',
                    fieldLabel: '停用碼',
                    store: st_ctdmd,
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 60,
                    width: 180,
                    queryMode: 'local',
                    multiSelect: true,
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    listConfig:
                    {
                        width: 180
                    },
                    matchFieldWidth: false
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        var MatClass = T1Query.getForm().findField('P2').getValue();//物料分類
                        if (MatClass == "01") { //藥品
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=FSTACKDATE]').setVisible(true);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=DAVG_USEQTY_90]').setVisible(true);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=SAFE_QTY_90]').setVisible(true);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=OPER_QTY_90]').setVisible(true);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=SHIP_QTY_90]').setVisible(true);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=HIGH_QTY_90]').setVisible(true); 
                        }
                        else {
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=FSTACKDATE]').setVisible(false);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=DAVG_USEQTY_90]').setVisible(false);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=SAFE_QTY_90]').setVisible(false);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=OPER_QTY_90]').setVisible(false);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=SHIP_QTY_90]').setVisible(false);
                            Ext.getCmp('Mi_winvctlGrid').down('[dataIndex=HIGH_QTY_90]').setVisible(false);
                        }
                        T1Load();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                        T1Query.getForm().findField('P3').setValue(arrP3);
                    }
                }
            ]
        }]
    });
    function chkWhNo() {
        var wh_no = T1Query.getForm().findField('P0').getValue();

        Ext.Ajax.request({
            url: '/api/AA0048/CheckWhid',
            params: {
                wh_no: T1Query.getForm().findField('P0').getValue()
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {

                } else {
                    T1Query.getForm().findField('P0').setValue('');
                    Ext.Msg.alert('訊息', data.msg);
                    return;
                }
            },
            failure: function (response, options) {

            }
        });
    }


    var T1Store = Ext.create('WEBAPP.store.MiWinvctl', { // 定義於/Scripts/app/store/MiUnitcode.js STATUS_DISPLAY
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件WH_NO的值代入參數
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

        if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
            (T1Query.getForm().findField('P2').getValue() == "" || T1Query.getForm().findField('P2').getValue() == null)) {

            Ext.Msg.alert('訊息', '需填庫房代碼及物料分類才能查詢');
            return;
        }
        msglabel('訊息區:');
        viewport.down('#form').collapse();

        T1Tool.moveFirst();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'add', text: '新增', id: 'add', hidden: true, handler: function () {
                    T1Set = '/api/AA0048/Create'; // AA0048Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', id: 'edit', disabled: true, handler: function () {
                    T1Set = '/api/AA0048/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'delete', text: '刪除', id: 'delete', disabled: true, hidden: true,
                handler: function () {
                    msglabel('訊息區:');
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0048/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            },
            {
                xtype: 'button',
                text: '匯出', handler: function () {
                    if ((T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) ||
                        (T1Query.getForm().findField('P2').getValue() == "" || T1Query.getForm().findField('P2').getValue() == null)) {

                        Ext.Msg.alert('訊息', '需填庫房代碼及物料分類才能查詢');
                    } else {
                        var p = new Array();
                        p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                        p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                        p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                        p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                        PostForm(T1GetExcel, p);
                        msglabel('訊息區:匯出完成');
                    }
                }
            },
            {
                xtype: 'filefield',
                name: 'filefield',
                displayfield: '上傳檔案',
                buttonText: '選擇檔案',
                margin: 5,
                id: 'select_file',
                width: 200,
                listeners: {
                    change: function (widget, value, eOpts) {
                        var files = event.target.files;
                        if (!files || files.length == 0) return; // make sure we got something
                        file = files[0];
                        var ext = this.value.split('.').pop();
                        if (!/^(xls|xlsx|XLS|XLSX)$/.test(ext)) {
                            Ext.MessageBox.alert('提示', '僅支持讀取xlsx和xls格式！');
                            T1Query.getForm().findField('filefield').fileInputEl.dom.value = '';
                            msglabel("訊息區:");
                        } else {
                            Ext.getCmp('check_mat').setDisabled(false);
                            Ext.getCmp('check_med').setDisabled(false);

                            msglabel("已選擇檔案");
                        }
                    }
                }
            },
            {
                xtype: 'component',
                width: '20px'
            },
            {
                xtype: 'button',
                text: '衛材更新',
                id: 'check_mat',
                name: 'check_mat',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var formData = new FormData();
                    formData.append("file", file);
                    var ajaxRequest = $.ajax({
                        type: "POST",
                        url: "/api/AA0048/CheckExcelMat",
                        data: formData,
                        processData: false,
                        //必須false才會自動加上正確的Content-Type
                        contentType: false,
                        success: function (data, textStatus, jqXHR) {
                            myMask.hide();
                            if (!data.success) {
                                T1Store.removeAll();
                                Ext.MessageBox.alert("提示", data.msg);
                                msglabel("訊息區:");
                            }
                            else {
                                Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                msglabel("訊息區:處理完成!");
                                T1Store.loadData(data.etts, false);
                                //Ext.getCmp('check').setDisabled(true);
                                //Ext.getCmp('insert').setDisabled(false);
                            }

                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }
            , {
                xtype: 'docbutton',
                text: '下載衛材範本',
                documentKey: 'AA0048_mat'
            },
            {
                xtype: 'component',
                width: '20px'
            },
            {
                xtype: 'button',
                text: '藥品更新',
                id: 'check_med',
                name: 'check_med',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    var formData = new FormData();
                    formData.append("file", file);
                    var ajaxRequest = $.ajax({
                        type: "POST",
                        url: "/api/AA0048/CheckExcelMed",
                        data: formData,
                        processData: false,
                        //必須false才會自動加上正確的Content-Type
                        contentType: false,
                        success: function (data, textStatus, jqXHR) {
                            myMask.hide();
                            if (!data.success) {
                                T1Store.removeAll();
                                Ext.MessageBox.alert("提示", data.msg);
                                msglabel("訊息區:");
                            }
                            else {
                                Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                                msglabel("訊息區:處理完成!");
                                T1Store.loadData(data.etts, false);
                                //Ext.getCmp('check').setDisabled(true);
                                //Ext.getCmp('insert').setDisabled(false);
                            }

                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            },
            , {
                xtype: 'docbutton',
                text: '下載藥品範本',
                documentKey: 'AA0048_med'
            },
        ]
    });

    function setFormT1(x, t) {          //做畫面隱藏等控制
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name2);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            T1Form.getForm().reset();
            var r = Ext.create('WEBAPP.model.MiWinvctl'); // /Scripts/app/model/MiWinvctl.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容;可問
            u = f.findField("WH_NO"); //在新增時才可填寫
            u.setReadOnly(false);

            f.findField('WH_NO').setReadOnly(false);
            f.findField('MMCODE').setReadOnly(false);
            f.findField('CTDMDCCODE').setValue('0');
            f.findField('USEADJ_CLASS').setValue('3');
        }
        else {
            u = f.findField('WH_NO');
        }
        f.findField('x').setValue(x);  //T1Form設定為I or U or D
        f.findField('SAFE_DAY').setReadOnly(false);
        f.findField('OPER_DAY').setReadOnly(false);
        f.findField('SHIP_DAY').setReadOnly(false);
        f.findField('HIGH_QTY').setReadOnly(false);
        f.findField('LOW_QTY').setReadOnly(false);
        f.findField('MIN_ORDQTY').setReadOnly(false);
        f.findField('IS_AUTO').setReadOnly(false);
        f.findField('USEADJ_CLASS').setReadOnly(false);
        f.findField('ISSPLIT').setReadOnly(false);
        
        if (x == 'I') {
            var whItem = wh_noStore.findRecord('VALUE', T1Query.getForm().findField('P0').getValue());
            hideDisplayField(whItem.data.EXTRA1, whItem.data.EXTRA2, x, u.getValue());
        }
        if (x == 'U') {
            hideDisplayField(T1LastRec.data.WH_KIND, T1LastRec.data.WH_GRADE, x, u.getValue());
        }

        //f.findField('CTDMDCCODE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    function hideDisplayField(wh_kind, wh_grade, type, wh_no) {
        var f = T1Form.getForm();
        // 衛材庫
        
        if (wh_kind == '1') {
            if (type == 'I') {
                f.findField('WH_NO').show();
                f.findField('MMCODE').show();
                f.findField('WH_NO_DISPLAY').hide();
                f.findField('MMCODE_DISPLAY').hide();
            }

            f.findField('SAFE_DAY').show();
            f.findField('OPER_DAY').show();
            f.findField('SHIP_DAY').show();
            f.findField('HIGH_QTY').show();
            f.findField('LOW_QTY').show();
            f.findField('MIN_ORDQTY').show();

            f.findField('SAFE_DAY_DISPLAY').hide();
            f.findField('OPER_DAY_DISPLAY').hide();
            f.findField('SHIP_DAY_DISPLAY').hide();
            f.findField('HIGH_QTY_DISPLAY').hide();
            f.findField('LOW_QTY_DISPLAY').hide();
            f.findField('MIN_ORDQTY_DISPLAY').hide();

            return;
        }

        // 藥品庫
        if (wh_grade == '2') {
            f.findField('USEADJ_CLASS').show();
            f.findField('IS_AUTO').show();
            f.findField('ISSPLIT').show();
            f.findField('USEADJ_CLASS_DISPLAY').hide();
            f.findField('IS_AUTO_DISPLAY').hide();
            f.findField('ISSPLIT_DISPLAY').hide();
        }
        if (wh_grade == '1') {
            f.findField('MIN_ORDQTY').show();
            f.findField('MIN_ORDQTY_DISPLAY').hide();
        }
        if (wh_no == 'PCA') {
            f.findField('USEADJ_CLASS').show();
            f.findField('IS_AUTO').show();
            f.findField('ISSPLIT').show();
            f.findField('USEADJ_CLASS_DISPLAY').hide();
            f.findField('IS_AUTO_DISPLAY').hide();
            f.findField('ISSPLIT_DISPLAY').hide();
        }

    }
    function showDisplayField() {
        var f = T1Form.getForm();
        f.findField('SAFE_DAY').hide();
        f.findField('OPER_DAY').hide();
        f.findField('SHIP_DAY').hide();
        f.findField('HIGH_QTY').hide();
        f.findField('LOW_QTY').hide();
        f.findField('MIN_ORDQTY').hide();
        f.findField('USEADJ_CLASS').hide();
        f.findField('IS_AUTO').hide();
        f.findField('ISSPLIT').hide();

        f.findField('SAFE_DAY_DISPLAY').show();
        f.findField('OPER_DAY_DISPLAY').show();
        f.findField('SHIP_DAY_DISPLAY').show();
        f.findField('HIGH_QTY_DISPLAY').show();
        f.findField('LOW_QTY_DISPLAY').show();
        f.findField('MIN_ORDQTY_DISPLAY').show();
        f.findField('USEADJ_CLASS_DISPLAY').show();
        f.findField('IS_AUTO_DISPLAY').show();
        f.findField('ISSPLIT_DISPLAY').show();
    }
    // 查詢結果資料列表AA0048
    var T1Grid = Ext.create('Ext.grid.Panel', {
        id: 'Mi_winvctlGrid', 
        store: T1Store, //資料load進來
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer',
            width: 30
        }, {
            text: "高警訊",
            dataIndex: 'DANGERDRUGFLAG_N',
            style: 'text-align:left',
            align: 'left',
            width: 70
        }, {
            text: "管制用藥",
            dataIndex: 'E_RESTRICTCODE_N',
            style: 'text-align:left',
            align: 'left',
            width: 70,
            renderer: function (val, meta, record) {
                switch (val) {
                    case "非管制用藥":
                        rtn = " ";
                        break;
                    case "其他列管藥品":
                        rtn = "他管";
                        break;
                    case "第一級管制用藥":
                        rtn = "管一";
                        break;
                    case "第二級管制用藥":
                        rtn = "管二";
                        break;
                    case "第三級管制用藥":
                        rtn = "管三";
                        break;
                    case "第四級管制用藥":
                        rtn = "管四";
                        break;
                }

                return rtn;
            }
        }, {
            text: "檢核結果",
            dataIndex: 'STATUS_DISPLAY',
            style: 'text-align:left',
            align: 'left',
            width: 150
        }, {
            text: "庫房代碼",
            dataIndex: 'WH_NO',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "名稱",
            dataIndex: 'MMNAME',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "最新效期",
            dataIndex: 'maxEXP_DATE',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "現有庫存",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "日平均消耗量",
            dataIndex: 'DAVG_USEQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "安全日",
            dataIndex: 'SAFE_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "安全量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "作業日",
            dataIndex: 'OPER_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "作業量",
            dataIndex: 'OPER_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "運補日",
            dataIndex: 'SHIP_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "運補量",
            dataIndex: 'SHIP_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "最低庫存量",
            dataIndex: 'LOW_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "儲位碼",
            dataIndex: 'STORE_LOC',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "是否自動撥補",
            dataIndex: 'IS_AUTO',
            style: 'text-align:left',
            align: 'left',
            width: 80
        }, {
            text: "最小撥補量",
            dataIndex: 'MIN_ORDQTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "扣庫單位",
            dataIndex: 'STOCKUNIT',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "各庫停用碼",
            dataIndex: 'CTDMDCCODE_N',
            style: 'text-align:left',
            align: 'left',
            width: 100
        }, {
            text: "撥發直接消耗",
            dataIndex: 'NOWCONSUMEFLAG',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "醫令扣庫歸整",
            dataIndex: 'USE_ADJCLASS_DISPLAY',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "是否拆單",
            dataIndex: 'ISSPLIT',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "第一次接收日期",
            dataIndex: 'FSTACKDATE',
            style: 'text-align:left',
            align: 'left',
            width: 120
        }, {
            text: "90天日平均消耗量",
            dataIndex: 'DAVG_USEQTY_90',
            style: 'text-align:left',
            align: 'right',
            width: 120
        }, {
            text: "安全量90",
            dataIndex: 'SAFE_QTY_90',
            style: 'text-align:left',
            align: 'right',
            width: 120
        }, {
            text: "作業量90",
            dataIndex: 'OPER_QTY_90',
            style: 'text-align:left',
            align: 'right',
            width: 120
        }, {
            text: "運補量90",
            dataIndex: 'SHIP_QTY_90',
            style: 'text-align:left',
            align: 'right',
            width: 120
        }, {
            text: "基準量90",
            dataIndex: 'HIGH_QTY_90',
            style: 'text-align:left',
            align: 'right',
            width: 120
        }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
                setFormT1a();
                if (T1LastRec != null) {
                    if (T1LastRec.data.WH_KIND == '0' && T1LastRec.data.WH_GRADE != '2' && T1LastRec.data.WH_GRADE != '1') {
                        Ext.getCmp('edit').hide();
                    } else {
                        Ext.getCmp('edit').show();
                    }
                    if (T1LastRec.data.WH_NO == 'PCA') {
                        Ext.getCmp('edit').show();
                    }
                }
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1RecLength === 0);      //新增完後T1RecLength是0，無法修改
        T1Grid.down('#delete').setDisabled(T1RecLength === 0);      //選擇左側T1Grid，一個會有T1LastRec，則可以修改

        //選擇左側T1Grid，一個會有T1LastRec，則可以修改
        if (T1LastRec) {
            viewport.down('#form').expand();
            isNew = false;
            
            T1Form.loadRecord(T1LastRec); //資料從T1Grid to T1Form
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('SAFE_DAY');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            
            if (T1LastRec.data.WH_KIND == '0' && T1LastRec.data.WH_GRADE == '2') {
                //Ext.getCmp('USEADJ_CLASS').show();
                Ext.getCmp('USEADJ_CLASS').allowBlank = false;
                Ext.getCmp('USEADJ_CLASS_DISPLAY').show();
            } else if (T1LastRec.data.WH_KIND == '0' && T1LastRec.data.WH_GRADE == '') {

            } else {
                // Ext.getCmp('USEADJ_CLASS').hide();
                Ext.getCmp('USEADJ_CLASS').allowBlank = true;
                Ext.getCmp('USEADJ_CLASS_DISPLAY').hide();
            }
        }
        else {
            T1Form.getForm().reset();  //右側資料清空
        }
    }

    var T1FormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'MMCODE',
        name: 'MMCODE',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        fieldCls: 'required',
        hidden: true,
        allowBlank: false, // 欄位為必填
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0048/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Form.getForm().findField('WH_NO').getValue()  //P0是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

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
        items: [
            { //控制項為了Update Insert Delete
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }
            , {
                xtype: 'combo',
                store: wh_noStore_insert,
                fieldLabel: '庫房代碼',
                name: 'WH_NO',
                id: 'WH_NO',
                queryMode: 'local',
                anyMatch: true,
                autoSelect: true,
                displayField: 'VALUE',
                valueField: 'VALUE',
                requiredFields: ['TEXT'],
                fieldCls: 'required',
                hidden: true,
                allowBlank: false, // 欄位為必填
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{VALUE}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{VALUE}</span><br/>&nbsp;<span style="color:blue">{TEXT}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {

                    }
                },
            },
            {
                xtype: 'displayfield',
                fieldLabel: '庫房代碼',
                name: 'WH_NO_DISPLAY',
                id: 'WH_NO_DISPLAY',
                sumbitValue: false
            },
            T1FormMMCode,
            {
                xtype: 'displayfield',
                fieldLabel: '院內碼',
                name: 'MMCODE_DISPLAY',
                id: 'MMCODE_DISPLAY',
                sumbitValue: false
            },
            {
                fieldLabel: '安全日',
                name: 'SAFE_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '安全日',
                name: 'SAFE_DAY_DISPLAY',
                id: 'SAFE_DAY_DISPLAY',
                sumbitValue: false
            },
            {
                fieldLabel: '作業日',
                name: 'OPER_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '作業日',
                name: 'OPER_DAY_DISPLAY',
                id: 'OPER_DAY_DISPLAY',
                sumbitValue: false
            }, {
                fieldLabel: '運補日',
                name: 'SHIP_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '運補日',
                name: 'SHIP_DAY_DISPLAY',
                id: 'SHIP_DAY_DISPLAY',
                sumbitValue: false
            }, {
                fieldLabel: '基準量',
                name: 'HIGH_QTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '基準量',
                name: 'HIGH_QTY_DISPLAY',
                id: 'HIGH_QTY_DISPLAY',
                sumbitValue: false
            }, {
                fieldLabel: '最低庫存量',
                name: 'LOW_QTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '最低庫存量',
                name: 'LOW_QTY_DISPLAY',
                id: 'LOW_QTY_DISPLAY',
                sumbitValue: false
            }, {
                fieldLabel: '最小撥補量',
                name: 'MIN_ORDQTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                hidden: true,
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '最小撥補量',
                name: 'MIN_ORDQTY_DISPLAY',
                id: 'MIN_ORDQTY_DISPLAY',
                sumbitValue: false
            }, {
                xtype: 'combo',
                fieldLabel: '是否自動撥補',
                store: st_isauto,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                queryMode: 'local',
                name: 'IS_AUTO',
                id: 'IS_AUTO',
                fieldCls: 'required',
                hidden: true,
                allowBlank: false, // 欄位為必填
                readOnly: true
            }, {
                xtype: 'displayfield',
                fieldLabel: '是否自動撥補',
                name: 'IS_AUTO_DISPLAY',
                id: 'IS_AUTO_DISPLAY',
                sumbitValue: false
            }, {
                xtype: 'combo',
                fieldLabel: '各庫停用碼',
                name: 'CTDMDCCODE',
                store: st_ctdmd,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                readOnly: true,
                typeAhead: true,
                forceSelection: true,
                hidden: true,
                queryMode: 'local',
                triggerAction: 'all'
            }, {
                xtype: 'displayfield',
                fieldLabel: '各庫停用碼',
                name: 'CTDMDCCODE_DISPLAY',
                id: 'CTDMDCCODE_DISPLAY',
                sumbitValue: false
            }, {
                xtype: 'combo',
                fieldLabel: '醫令扣庫歸整',
                name: 'USEADJ_CLASS',
                id: 'USEADJ_CLASS',
                store: st_useadj_class,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                fieldCls: 'required',
                allowBlank: true, // 欄位為必填
                readOnly: true,
                typeAhead: true,
                forceSelection: true,
                hidden: true,
                queryMode: 'local',
                triggerAction: 'all'
            }
            , {
                xtype: 'displayfield',
                fieldLabel: '醫令扣庫規整',
                name: 'USEADJ_CLASS_DISPLAY',
                id: 'USEADJ_CLASS_DISPLAY',
                sumbitValue: false
            }, {
                xtype: 'combo',
                fieldLabel: '是否拆單',
                store: st_isauto,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                queryMode: 'local',
                name: 'ISSPLIT',
                id: 'ISSPLIT',
                fieldCls: 'required',
                hidden: true,
                allowBlank: false, // 欄位為必填
                readOnly: true,
                labelWidth: 90
            }, {
                xtype: 'displayfield',
                fieldLabel: '是否拆單',
                name: 'ISSPLIT_DISPLAY',
                id: 'ISSPLIT_DISPLAY',
                sumbitValue: false,
                labelWidth: 90
            }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
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
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: function () {

                T1Cleanup();
            }
        }]
    });

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,  //導到後端API
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) { //insert update 問
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            r.commit();
                            //T1Query.getForm().findField('P0').setValue(f2.findField('WH_NO').getValue());
                            //T1Query.getForm().findField('P1').setValue(f2.findField('MMCODE').getValue());
                            T1Query.getForm().findField('P1').focus();
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料修改成功');
                            break;
                        case "D":
                            T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                            r.commit();
                            T1Load();
                            msglabel('訊息區:資料刪除成功');
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
        viewport.down('#form').collapse();
        viewport.down('#t1Grid').unmask(); //左邊refresh結束
        var f = T1Form.getForm();
        //f.reset(); //在 setFormT1a 有設定
        f.getFields().each(function (fc) { //右側欄位Read Only
            if (fc.xtype == "displayfield" || fc.xtype == "textfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        showDisplayField();
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        T1Query.getForm().findField('P3').setValue(arrP3);
        setFormT1a();
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
            region: 'east',
            collapsible: true,
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

    //T1Load(); // 進入畫面時自動載入一次資料
    //T1Query.getForm().findField('P0').focus();
    st_isauto.add({ VALUE: "Y", COMBITEM: "Y" });
    st_isauto.add({ VALUE: "N", COMBITEM: "N" });
    T1Query.getForm().findField('P3').setValue(arrP3);

    viewport.down('#form').collapse();
});