Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/CB0005/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "品項管理員資料";
    var WhnoGet = '../../../api/CB0005/GetWhno';
    var UserIdGet = '../../../api/CB0005/GetUserId';

    var T1Rec = 0;
    var T1LastRec = null;
    var whno_temp = "";

    var P0 = '';
    var P1 = '';
    var P2 = '';


    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var WhnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    //預設UserWhno
    function setDefaultWhno() {
        Ext.Ajax.request({
            url: WhnoGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var whnos = data.etts;
                    if (whnos.length > 0) {
                        for (var i = 0; i < whnos.length; i++) {
                            WhnoQueryStore.add({ VALUE: whnos[i].VALUE, TEXT: whnos[i].TEXT });
                            whno_temp = whnos[0].VALUE;
                        }
                        T1Query.getForm().findField('P0').setValue(whno_temp);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 250;
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
                    fieldLabel: '庫房別',
                    store: WhnoQueryStore,
                    name: 'P0',
                    id: 'P0',
                    // enforceMaxLength: true, // 限制可輸入最大長度
                    // maxLength: 100, // 可輸入最大長度為100
                    padding: '0 4 0 4',
                    labelWidth: 50,
                    width: 210,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    autoSelect: true,
                    anyMatch: true,
                    multiSelect: false,
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    allowBlank: false, // 欄位為必填
                    blankText: "請選擇庫房別",
                    fieldCls: 'required'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '管理人員代號',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '管理人員敘述',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel("");
                        if (T1Query.getForm().isValid()) {
                            P0 = T1Query.getForm().findField('P0').getValue();
                            P1 = T1Query.getForm().findField('P1').getValue();
                            P2 = T1Query.getForm().findField('P2').getValue();

                            T1Load();
                        }
                        else {
                            Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>為必填');
                            msglabel(" <span style='color:red'>庫房別</span>為必填");
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        msglabel("");
                        f.reset();
                        T1Query.getForm().findField('P0').setValue(whno_temp);
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    }
                }
            ]
        }]
    });

    var T1Store = Ext.create('WEBAPP.store.BcManager', { // 定義於/Scripts/app/store/BcManager.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P2的值代入參數
                var np = {
                    p0: P0,
                    p1: P1,
                    p2: P2
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
                text: '新增', handler: function () {
                    T1Set = '/api/CB0005/Create'; // CB0005Controller的Create
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/CB0005/Update';
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    msglabel("");
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/CB0005/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    });
                }
            },
            {
                itemId: 'export', text: '匯出', disabled: false,
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            p.push({ name: 'P0', value: P0 }); //SQL篩選條件
                            p.push({ name: 'P1', value: P1 }); //SQL篩選條件
                            p.push({ name: 'P2', value: P2 }); //SQL篩選條件
                            PostForm('/api/CB0005/Excel', p);
                        }
                    });
                }
            }
        ]
    });

    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        T1Query.getForm().findField('P0').clearInvalid();
        var f = T1Form.getForm();
        msglabel("");
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.BcManager'); // /Scripts/app/model/BcManager.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("WH_NO"); // 庫房別在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            w = f.findField("MANAGERID"); // 管理人員代號在新增時才可填寫
            w.setReadOnly(false);
            w.clearInvalid();
            w = f.findField("MANAGERNM");
            w.clearInvalid();
            w = f.findField("USERID");
            w.clearInvalid();

            setCmpShowCondition(true, false, false, true, false, false, true, false, true, false, false);
        }
        else {
            u = f.findField('MANAGERNM');

            setCmpShowCondition(false, true, false, false, true, false, true, false, true, false, false);

        }
        f.findField('x').setValue(x);
        f.findField('MANAGERNM').setReadOnly(false);
        f.findField('USERID').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();//指定游標停留在u指定的欄位
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
        },
        {
            text: "庫房別",
            dataIndex: 'WH_NO',
            width: 70
        }, {
            text: "管理人員代號",
            dataIndex: 'MANAGERID',
            width: 90
        }, {
            text: "管理人員敘述",
            dataIndex: 'MANAGERNM',
            width: 150
        }, {
            text: "管理人員",
            dataIndex: 'MNAME',
            width: 110
        }, {
            text: "管理品項數",
            dataIndex: 'CNT',
            width: 90,
            align: 'right',
            style: 'text-align:left',
            xtype: 'numbercolumn',
            format: '0'
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Tool.down('#export').setDisabled(T1Store.getCount() === 0);
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Query.getForm().findField('P0').clearInvalid();
                T1Rec = records.length;
                T1LastRec = records[0];
                viewport.down('#form').expand();
                setFormT1a();
                if (T1LastRec) {
                    msglabel("");
                }
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
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

    // 管理人員帳號清單
    var useridQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    function setUserId() {
        Ext.Ajax.request({
            url: UserIdGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var userid = data.etts;
                    if (userid.length > 0) {
                        for (var i = 0; i < userid.length; i++) {
                            useridQueryStore.add({ VALUE: userid[i].VALUE, TEXT: userid[i].TEXT });
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
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
        items: [{
            fieldLabel: 'Update',
            name: 'x',
            xtype: 'hidden'
        },
        {
            xtype: 'combo',
            fieldLabel: '庫房別',
            store: WhnoQueryStore,
            name: 'WH_NO',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            maxLength: 80,
            autoSelect: true,
            anyMatch: true,
            multiSelect: false,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請輸入庫房別",
            allowBlank: false,
            readOnly: true,
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫房別', //白底顯示
            name: 'DISPLAY_WHNO',
            enforceMaxLength: true,
            maxLength: 80,
            readOnly: true
        }, {
            fieldLabel: '庫房別', //紅底顯示
            name: 'TEXT_WHNO',
            enforceMaxLength: true,
            maxLength: 80,
            readOnly: true,
            fieldCls: 'required'
        },
        {
            fieldLabel: '管理人員代號',
            name: 'MANAGERID',
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
            blankText: "請輸入管理人員代號"
        }, {
            xtype: 'displayfield',
            fieldLabel: '管理人員代號',//白底顯示
            name: 'DISPLAY_MANAGERID',
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true
        }, {
            fieldLabel: '管理人員代號',//紅底顯示
            name: 'TEXT_MANAGERID',
            enforceMaxLength: true,
            maxLength: 10,
            readOnly: true,
            fieldCls: 'required'
        },
        {
            fieldLabel: '管理人員敘述',
            name: 'MANAGERNM',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',
            blankText: "請輸入管理人員敘述"
        }, {
            fieldLabel: '管理人員敘述',//紅底顯示
            name: 'TEXT_MANAGERNM',
            enforceMaxLength: true,
            maxLength: 20,
            readOnly: true,
            fieldCls: 'required'
        },
        {
            xtype: 'combo',
            store: useridQueryStore,
            fieldLabel: '管理人員帳號',
            name: 'USERID',
            displayField: 'TEXT',
            valueField: 'VALUE',
            queryMode: 'local',
            autoSelect: true,
            anyMatch: true,
            multiSelect: false,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇管理人員帳號",
            allowBlank: false,
            readOnly: true,
            fieldCls: 'required',
            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
        },
        {
            fieldLabel: '管理人員帳號',
            name: 'MNAME',
            enforceMaxLength: true,
            maxLength: 100,
            readOnly: true,
            fieldCls: 'required'
        }, {
            xtype: 'displayfield',
            fieldLabel: '管理品項數',//白底顯示
            name: 'CNT',
            readOnly: true
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if ((T1Form.getForm().findField("WH_NO").getValue() == null) ||
                    (T1Form.getForm().findField("WH_NO").getValue() == "")) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>庫房別不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>庫房別不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("MANAGERID").getValue().trim() == null) ||
                    (T1Form.getForm().findField("MANAGERID").getValue().trim() == "")) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>管理人員代號不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>管理人員代號不可為空</span>，請重新輸入。");
                }
                else if ((T1Form.getForm().findField("MANAGERNM").getValue().trim() == null) ||
                    (T1Form.getForm().findField("MANAGERNM").getValue().trim() == "")) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>管理人員敘述不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>管理人員敘述不可為空</span>，請重新輸入。");
                } else if ((T1Form.getForm().findField("USERID").getValue() == null) ||
                    (T1Form.getForm().findField("USERID").getValue() == "")) {
                    Ext.Msg.alert('提醒', "<span style='color:red'>管理人員帳號不可為空</span>，請重新輸入。");
                    msglabel(" <span style='color:red'>管理人員帳號不可為空</span>，請重新輸入。");
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

    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    setCmpShowCondition(false, false, true, false, false, true, false, true, false, true, true);
                    T1Load();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    var whnovalue = T1Form.getForm().findField("WH_NO").getValue();
                    var manageridvalue = T1Form.getForm().findField("MANAGERID").getValue();
                    var managernmvalue = T1Form.getForm().findField("MANAGERNM").getValue();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            var v = action.result.etts[0];
                            r.set(v);
                            T1Store.insert(0, r);
                            msglabel(" 資料新增成功");
                            r.commit();
                            T1Query.getForm().findField("P0").setValue(whnovalue);
                            T1Query.getForm().findField("P1").setValue(manageridvalue);
                            T1Query.getForm().findField("P2").setValue(managernmvalue);
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
        setCmpShowCondition(false, false, true, false, false, true, false, true, false, true, true);
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
            collapsed: true,
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

    //控制不可更改項目的顯示
    function setCmpShowCondition(wh_no, display_whno, text_whno, managerid, display_managerid, text_managerid, managernm, text_managernm, userid, mname, cnt) {
        var f = T1Form.getForm();
        wh_no ? showComponent(f, "WH_NO") : hideComponent(f, "WH_NO");
        display_whno ? showComponent(f, "DISPLAY_WHNO") : hideComponent(f, "DISPLAY_WHNO");
        text_whno ? showComponent(f, "TEXT_WHNO") : hideComponent(f, "TEXT_WHNO");
        managerid ? showComponent(f, "MANAGERID") : hideComponent(f, "MANAGERID");
        display_managerid ? showComponent(f, "DISPLAY_MANAGERID") : hideComponent(f, "DISPLAY_MANAGERID");
        text_managerid ? showComponent(f, "TEXT_MANAGERID") : hideComponent(f, "TEXT_MANAGERID");
        managernm ? showComponent(f, "MANAGERNM") : hideComponent(f, "MANAGERNM");
        text_managernm ? showComponent(f, "TEXT_MANAGERNM") : hideComponent(f, "TEXT_MANAGERNM");
        userid ? showComponent(f, "USERID") : hideComponent(f, "USERID");
        mname ? showComponent(f, "MNAME") : hideComponent(f, "MNAME");
        cnt ? showComponent(f, "CNT") : hideComponent(f, "CNT");
    }

    //T1Load(); // 進入畫面時自動載入一次資料
    setCmpShowCondition(false, false, true, false, false, true, false, true, false, true, true);
    setDefaultWhno();
    T1Query.getForm().findField('P0').focus(); //讓游標停在P0這一格
    setUserId();
});
