Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name1 = "安全量、作業量維護";
    var T1Name2 = "安全量、作業量";


    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var WH_NOComboGet = '../../../api/AA0048/GetWH_NOComboNotOne'; //庫別級別(1庫 2局(衛星庫) 3病房 4科室 5戰備庫)

    // 庫房代碼
    var wh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
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
                    if (tb_wh_no.length == 1) {
                        //1筆資料時將
                        combo_P0.setValue(tb_wh_no[0].WH_NO);
                    }
                    else {
                        combo_P0.setDisabled(false);
                    }
                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            wh_noStore.add({ WH_NO: tb_wh_no[i].WH_NO, WH_NAME: tb_wh_no[i].WH_NAME });

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
        width: 300,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0048/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1Query.getForm().findField('P0').getValue()  //P0是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    var mLabelWidth = 90;
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
                    displayField: 'WH_NO',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
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
                    padding: '0 4 0 4'
                },
                T1QuryMMCode,
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });


    var T1Store = Ext.create('WEBAPP.store.MiWinvctl', { // 定義於/Scripts/app/store/MiUnitcode.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件WH_NO的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        if (T1Query.getForm().findField('P0').getValue() == "" || T1Query.getForm().findField('P0').getValue() == null) {

            Ext.Msg.alert('訊息', '需填庫房代碼才能查詢');
        } else {
            T1Tool.moveFirst();
        }
        msglabel('訊息區:');
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store, //資料load進來
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/AA0048/Create'; // AA0048Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0048/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'delete', text: '刪除', disabled: true,
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
            }
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
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: T1Name1,
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
            text: "安全日",
            dataIndex: 'SAFE_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "作業日",
            dataIndex: 'OPER_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "運補日",
            dataIndex: 'SHIP_DAY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "基準量",
            dataIndex: 'HIGH_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }, {
            text: "最低庫存量",
            dataIndex: 'LOW_QTY',
            style: 'text-align:left',
            align: 'right',
            width: 80
        }, {
            text: "最小撥補量",
            dataIndex: 'MIN_ORDQTY',
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
            text: "作業量",
            dataIndex: 'OPER_QTY',
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
            text: "日平均消耗量",
            dataIndex: 'DAVG_USEQTY',
            style: 'text-align:left',
            align: 'right',
            width: 100
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1RecLength = records.length;
                T1LastRec = records[0];
                setFormT1a();
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
                store: wh_noStore,
                name: 'WH_NO',
                fieldLabel: '庫房代碼',
                queryMode: 'local',
                anyMatch: true,
                readOnly: true,
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                autoSelect: true,
                displayField: 'WH_NO',
                valueField: 'WH_NO',
                requiredFields: ['WH_NAME'],
                tpl: new Ext.XTemplate(
                    '<tpl for=".">',
                    '<tpl if="VALUE==\'\'">',
                    '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                    '<tpl else>',
                    '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                    '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                    '</tpl></tpl>', {
                        formatText: function (text) {
                            return Ext.util.Format.htmlEncode(text);
                        }
                    }),
                editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                listeners: {
                    beforequery: function (record) {
                        record.query = new RegExp(record.query, 'i');
                        record.forceAll = true;
                    },
                    select: function (combo, records, eOpts) {
                    }
                }
            },
            T1FormMMCode,
            {
                fieldLabel: '安全日',
                name: 'SAFE_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            }, {
                fieldLabel: '作業日',
                name: 'OPER_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            }, {
                fieldLabel: '運補日',
                name: 'SHIP_DAY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            }, {
                fieldLabel: '基準量',
                name: 'HIGH_QTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            }, {
                fieldLabel: '最低庫存量',
                name: 'LOW_QTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            }, {
                fieldLabel: '最小撥補量',
                name: 'MIN_ORDQTY',
                enforceMaxLength: true,
                regexText: '只能輸入非零正數',
                minValue: 0, // 用正規表示式限制可輸入內容
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                maxLength: 20,
                readOnly: true
            },],
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
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
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
                            T1Query.getForm().findField('P0').setValue(f2.findField('WH_NO').getValue());
                            T1Query.getForm().findField('P1').setValue(f2.findField('MMCODE').getValue());
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
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
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
    viewport.down('#form').collapse();
});