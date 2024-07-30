﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var miWhmastComboGet = '../../../api/AA0047/GetmiWhmastCombo';

    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "單位撥發時間設定";

    var T1Rec = 0;
    var T1LastRec = null;

    var T1GetExcel = '../../../api/AA0047/Excel';
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var form_recStatus = Ext.create('Ext.data.Store', {
        fields: ['KEYCODE', 'VALUE'],
        data: [
            { "KEYCODE": "A", "NAME": "啟用", "COMBITEM": "A:啟用" },
            { "KEYCODE": "X", "NAME": "停用", "COMBITEM": "X:停用" }
        ]
    });

    // 庫房代碼
    var miWhmastQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM']
    });

    function setComboData() {

        Ext.Ajax.request({
            url: miWhmastComboGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_miWhmast = data.etts;
                    if (tb_miWhmast.length > 0) {
                        for (var i = 0; i < tb_miWhmast.length; i++) {
                            miWhmastQueryStore.add({ KEY_CODE: tb_miWhmast[i].KEY_CODE, COMBITEM: tb_miWhmast[i].COMBITEM });
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
                    xtype: 'monthfield',
                    fieldLabel: '撥發年月',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4'
                },
                //{
                //    xtype: 'textfield',
                //    fieldLabel: '庫房代碼',
                //    name: 'P1',
                //    id: 'P1',
                //    enforceMaxLength: true, // 限制可輸入最大長度
                //    maxLength: 100, // 可輸入最大長度為100
                //    padding: '0 4 0 4'
                //},
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        T1Load();
                        msglabel('訊息區:');
                    }
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
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.MmWhapldt', { // 定義於/Scripts/app/store/MmWhapldt.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getRawValue(),
                    //p1: T1Query.getForm().findField('P1').getValue(),
                    //p2: T1Query.getForm().findField('P2').getValue(),
                    //p3: T1Query.getForm().findField('P3').getValue(),
                    //p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});

        T1Grid.columns[1].setVisible(false);
        T1Grid.columns[2].setVisible(false);
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
                    T1Set = '/api/AA0047/Create'; // AA0047Controller的Create
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0047/Update';
                    setFormT1("U", '修改');
                }
            }
            , {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            T1Set = '/api/AA0047/Delete';
                            T1Form.getForm().findField('x').setValue('D');
                            T1Submit();
                        }
                    }
                    );
                }
            }, {
                id: 't1export', text: '範本', disabled: false, handler: function () {
                    var p = new Array();
                    //p.push({ name: 'FN', value: today + '_新合約品項批次更新_物料主檔.xls' });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }, {
                xtype: 'filefield',
                name: 'send',
                id: 'send',
                buttonOnly: true,
                buttonText: '匯入',
                width: 40,
                listeners: {
                    change: function (widget, value, eOpts) {
                        //Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        Ext.getCmp('insert').setDisabled(true);
                        T1Store.removeAll();
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
                                url: "/api/AA0047/SendExcel",
                                data: formData,
                                processData: false,
                                //必須false才會自動加上正確的Content-Type
                                contentType: false,
                                success: function (data, textStatus, jqXHR) {
                                    if (!data.success) {
                                        T1Store.removeAll();
                                        Ext.MessageBox.alert("提示", data.msg);
                                        msglabel("訊息區:");
                                        Ext.getCmp('insert').setDisabled(true);
                                        IsSend = false;
                                    }
                                    else {
                                        msglabel("訊息區:檔案讀取成功");
                                        T1Store.loadData(data.etts, false);
                                        IsSend = true;
                                        T1Grid.columns[1].setVisible(true);
                                        T1Grid.columns[2].setVisible(true);
                                        if (data.msg == "True") {
                                            Ext.getCmp('insert').setDisabled(false);
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: blue; font-weight: bold\">成功</span>，可進行更新動作。");
                                        };
                                        if (data.msg == "False") {
                                            Ext.MessageBox.alert("提示", "檢核<span style=\"color: red; font-weight: bold\">失敗</span>，請依錯誤說明修改Excel檔。");
                                        };
                                    }
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    myMask.hide();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    Ext.Msg.alert('失敗', 'Ajax communication failed');
                                    Ext.getCmp('send').fileInputEl.dom.value = '';
                                    Ext.getCmp('insert').setDisabled(true);
                                    myMask.hide();

                                }
                            });
                        }
                    }
                }
            },
            {
                text: '更新',
                id: 'insert',
                name: 'insert',
                disabled: true,
                handler: function () {
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();
                    Ext.Ajax.request({
                        url: '/api/AA0047/Insert',
                        method: reqVal_p,
                        params: {
                            data: Ext.encode(Ext.pluck(T1Store.data.items, 'data'))
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                //T1Store.loadData(data.etts, false);
                                if (data.msg == "True") {
                                    Ext.MessageBox.alert("提示", "<span style=\"color: red; font-weight: bold\">庫房</span>不可重複，請修改Excel檔。");
                                    msglabel("訊息區:<span style=\"color: red; font-weight: bold\">庫房</span>不可重複，請修改Excel檔。");
                                }
                                else {
                                    Ext.MessageBox.alert("提示", "主檔更新<span style=\"color: blue; font-weight: bold\">完成</span>。");
                                    msglabel("訊息區:主檔更新<span style=\"color: red; font-weight: bold\">完成</span>");
                                }
                                Ext.getCmp('insert').setDisabled(true);
                                T1Store.removeAll();
                                T1Grid.columns[1].setVisible(false);
                                T1Grid.columns[2].setVisible(false);
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
            }
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        var q = T1Query.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MmWhapldt'); // /Scripts/app/model/MmWhapldt.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("APPLY_YEAR_MONTH"); // 撥發日在新增時才可填寫

            u.setValue(q.findField('P0').getRawValue());   //20190415 預設帶查詢的年月條件
            u.setReadOnly(false);
            u.clearInvalid();
            u.focus();      //20190415  預設focus在撥發年月
            u = f.findField("APPLY_DAY"); // 撥發日在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            u = f.findField("WH_NO"); // 撥發日在新增時才可填寫
            u.clearInvalid();

            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
        }
        else {
            u = f.findField('WH_NO');
            u.focus();
        }
        f.findField('x').setValue(x);
        f.findField('WH_NO').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,    //0409會議決定不顯示
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
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "檢核結果",
                dataIndex: 'CHECK_RESULT',
                hidden: true,
                width: 250
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                hidden: true,
                width: 150
            }, {     
                text: "撥發年月",
                dataIndex: 'APPLY_DATE',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val) {
                    return Ext.util.Format.substr(val, 0, 5);
                }
            }, {
                text: "撥發日",
                dataIndex: 'APPLY_DATE',
                width: 80,
                style: 'text-align:left',
                align: 'right',
                renderer: function (val) {
                    return Ext.util.Format.substr(val, 5, 2);
                }
            }, {
                text: "庫房",
                dataIndex: 'WH_NO_N',
                width: 350
            }
        ],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
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
            var u = f.findField('APPLY_YEAR_MONTH');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            var u = f.findField('APPLY_DAY');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            var u = f.findField('WH_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');

            //將庫房代碼放入暫存欄位
            f.findField('WH_NO_OLD').setValue(T1LastRec.get('WH_NO'));

            //if (T1LastRec.data['REC_STATUS'] == 'X')
            //    T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
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
        }, {    //庫房代碼暫存欄位
            xtype: 'hidden',
            name: 'WH_NO_OLD'
        }, {
            xtype: 'monthfield',
            fieldLabel: '撥發年月',
            name: 'APPLY_YEAR_MONTH',
            enforceMaxLength: true,
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }, {
            fieldLabel: '撥發日',
            name: 'APPLY_DAY',
            enforceMaxLength: true,
            maxLength: 2,
            regexText: '只能輸入2位數字ex.01',
            regex: /^\d{2}$/, // 用正規表示式限制可輸入內容
            readOnly: true,
            allowBlank: false, // 欄位為必填
            fieldCls: 'required'
        }, {
            xtype: 'combo',
            store: miWhmastQueryStore,
            displayField: 'COMBITEM',
            valueField: 'KEY_CODE',
            multiSelect: false,
            delimiter: ',',
            id: 'WH_NO',
            name: 'WH_NO',
            fieldLabel: '庫房代碼',
            allowBlank: false, // 欄位為必填
            fieldCls: 'required',   //20190415  為必填欄位
            queryMode: 'local',
            readOnly: true,
            autoSelect: true,
            editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: false,
            listConfig: { width: 220 },
            listeners: {
                beforequery: function (record) {
                    record.query = new RegExp(record.query, 'i');
                    record.forceAll = true;
                },
                expand: function (field, eOpts) {
                    if (field.value) {
                        var miWhmastL = field.value;
                        field.select(miWhmastL);
                    }
                }
            }
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                    //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                    //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                    //else
                    //{
                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                    //}
                }
                else
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
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
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            //新增完後自動帶入最新一筆新增的年月資料到查詢條件，清空撥發日與庫房代碼，且重新LoadGrid
                            T1Query.getForm().findField('P0').setValue(f2.findField('APPLY_YEAR_MONTH').getValue());
                            f2.findField('APPLY_DAY').setValue('');
                            f2.findField('WH_NO').setValue('');
                            T1Load();
                            f2.findField('APPLY_DAY').clearInvalid();       //撥發日檢核提示先取消
                            f2.findField('WH_NO').clearInvalid();           //庫房代碼檢核提示先取消
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            T1Query.getForm().findField('P0').setValue(f2.findField('APPLY_YEAR_MONTH').getValue());
                            f2.findField('APPLY_DAY').setValue('');
                            f2.findField('WH_NO').setValue('');
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
                    T1Cleanup(f2.findField("x").getValue());
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
    function T1Cleanup(type) {
        if (type == "I") {

        }
        else {
            viewport.down('#t1Grid').unmask();
            var f = T1Form.getForm();
            f.reset();
            f.getFields().each(function (fc) {
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
    T1Query.getForm().findField('P0').focus();
});
