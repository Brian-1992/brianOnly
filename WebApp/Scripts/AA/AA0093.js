Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


// -- 建立下拉選單 - -
var months = Ext.create('Ext.data.Store', {
    fields: ['abbr', 'name'],
    data: [
        { "TEXT": "1", "VALUE": "1" },
        { "TEXT": "2", "VALUE": "2" },
        { "TEXT": "3", "VALUE": "3" },
        { "TEXT": "4", "VALUE": "4" },
        { "TEXT": "5", "VALUE": "5" },
        { "TEXT": "6", "VALUE": "6" },
        { "TEXT": "7", "VALUE": "7" },
        { "TEXT": "8", "VALUE": "8" },
        { "TEXT": "9", "VALUE": "9" },
        { "TEXT": "10", "VALUE": "10" },
        { "TEXT": "11", "VALUE": "11" },
        { "TEXT": "12", "VALUE": "12" }
    ]
});




Ext.onReady(function () {
    // var T1Get = '/api/BE0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "廠商基本檔維護";

    var T1Rec = 0;
    var T1LastRec = null;

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    // 院內碼清單
    var mmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var WhnoComboGet = '../../../api/AA0093/GetWhnoCombo';
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        // whnoQueryStore.add({ VALUE: '', TEXT: '' });
                        var wh_no = null;
                        for (var i = 0; i < wh_nos.length; i++) {
                            if (wh_no == null) {
                                wh_no = wh_nos[i].VALUE;
                            }
                            whnoQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        var combo = T1Query.getForm().findField('P0');
                        combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料

                        //setMmcodeCombo(wh_no);
                    }
                    else {
                        Ext.MessageBox.alert('錯誤', '查不到你所屬庫房資料');
                        msglabel('訊息區:查不到你所屬庫房資料');
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    //var MmcodeComboGet = '../../../api/AA0093/GetMmcodeCombo/';
    //function mmcodeComboPromise(wh_no) {
    //    var deferred = new Ext.Deferred();

    //    Ext.Ajax.request({
    //        url: MmcodeComboGet,
    //        method: reqVal_p,
    //        params: {
    //            WH_NO: wh_no
    //        },
    //        success: function (response) {
    //            deferred.resolve(response.responseText);
    //        },

    //        failure: function (response) {
    //            deferred.reject(response.status);
    //        }
    //    });

    //    return deferred.promise; //will return the underlying promise of deferred
    //}
    //function setMmcodeCombo(wh_no) {
    //    var promise = mmcodeComboPromise(wh_no);
    //    promise.then(function (success) {
    //        var data = JSON.parse(success);
    //        if (data.success) {
    //            var mmcodes = data.etts;
    //            mmcodeQueryStore.removeAll();
    //            if (mmcodes.length > 0) {
    //                mmcodeQueryStore.add({ VALUE: '', TEXT: '' });
    //                for (var i = 0; i < mmcodes.length; i++) {
    //                    mmcodeQueryStore.add({ VALUE: mmcodes[i].VALUE, TEXT: mmcodes[i].TEXT });
    //                }
    //            }
    //            //setFormT1a();
    //        }
    //    }
    //    )
    //}

    function showReport() {
        if (!win) {
            if (
                T1Query.getForm().findField('P0').getValue() != null &&
                T1Query.getForm().findField('P1').getValue() != null &&
                true
            ) {
                var winform = Ext.create('Ext.form.Panel', {
                    id: 'iframeReport',
                    //height: '100%',
                    //width: '100%',
                    layout: 'fit',
                    closable: false,
                    html: '<iframe src="' + reportUrl + '?'
                    + '&p0=' + T1Query.getForm().findField('P0').getValue()
                    + '&p1=' + T1Query.getForm().findField('P1').getValue()
                    + '&inid=' + session['Inid'] + ' ' + session['InidName']
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                    buttons: [{
                        text: '關閉',
                        handler: function () {
                            this.up('window').destroy();
                        }
                    }]
                });
                var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
                win.show();
            }
            else {
                Ext.Msg.alert('提醒', '庫房代碼 與 產生少於n個月效期報表 必填');
                msglabel('訊息區:庫房代碼 與 產生少於n個月效期報表 必填');
            }
        }
    }

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var form_recStatus = Ext.create('Ext.data.Store', {
        fields: ['KEYCODE', 'VALUE'],
        data: [
            { "KEYCODE": "A", "NAME": "啟用", "COMBITEM": "A 啟用" },
            { "KEYCODE": "X", "NAME": "停用", "COMBITEM": "X 停用" }
        ]
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
                    store: whnoQueryStore,
                    name: 'P0',
                    id: 'P0',
                    fieldLabel: '庫房代碼',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    queryMode: 'local',
                    triggerAction: 'all',
                    fieldCls: 'required',
                    multiSelect: false,
                    blankText: "請選擇庫房號碼",
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    padding: '0 4 0 4',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            var wh_no = newValue.data.VALUE;
                        }
                    }
                }, {
                    xtype: 'combo',
                    store: months, // 
                    fieldLabel: '產生少於', // 
                    queryMode: 'local', // 
                    displayField: 'TEXT', // 
                    valueField: 'VALUE', // 
                    name: 'P1',
                    id: 'P1',

                    anyMatch: true,
                    allowBlank: false, // 欄位為必填
                    typeAhead: true,
                    forceSelection: true,
                    triggerAction: 'all',
                    fieldCls: 'required',
                    multiSelect: false,
                    blankText: "請選擇月數",
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    padding: '0 4 0 4',
                    listeners: {
                        select: function (oldValue, newValue, eOpts) {
                            //var wh_no = newValue.data.VALUE;
                        }
                    }
                }, {
                    xtype: 'label',
                    text: '個月效期報表',
                    margin: '0 4 0 4'
                    //,name: 'PICK_DATE'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    iconCls: 'TRASearch',
                    handler: function () {
                        if (
                            T1Query.getForm().findField('P0').getValue() != null &&
                            T1Query.getForm().findField('P1').getValue() != null &&
                            true
                        ) {
                            Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                            T1Load();
                            msglabel('訊息區:');
                        }
                        else
                        {
                            Ext.Msg.alert('提醒', '庫房代碼 與 產生少於n個月效期報表 必填');
                            msglabel('訊息區:庫房代碼 與 產生少於n個月效期報表 必填');
                        }
                    }
                }, {
                    xtype: 'button',
                    text: '列印',
                    handler: function () {
                        reportUrl = '/Report/A/AA0093.aspx';
                        showReport();
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    iconCls: 'TRAClear',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP4',
            border: false,
            layout: 'hbox',
            items: [

            ]
        }, {
            xtype: 'panel',
            id: 'PanelP5',
            border: false,
            layout: 'hbox',
            items: [
                // -- 

            ]
        }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0093', { // 定義於/Scripts/app/store/PhVender.js
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue()
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
            //{
            //    text: '新增', handler: function () {
            //        T1Set = '/api/BE0002/Create'; // BE0002Controller的Create
            //        msglabel('訊息區:');
            //        setFormT1('I', '新增');
            //    }
            //},
            //{
            //    itemId: 'edit', text: '修改', disabled: true, handler: function () {
            //        T1Set = '/api/BE0002/Update';
            //        msglabel('訊息區:');
            //        setFormT1("U", '修改');
            //    }
            //}
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/BE0002/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        viewport.down('#form').expand();
        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhVender'); // /Scripts/app/model/PhVender.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
        }
        else {
            u = f.findField('AGEN_NAMEC');
        }
        f.findField('x').setValue(x);
        f.findField('AGEN_NAMEC').setReadOnly(false);
        f.findField('AGEN_NAMEE').setReadOnly(false);
        f.findField('AGEN_ADD').setReadOnly(false);
        f.findField('AGEN_FAX').setReadOnly(false);
        f.findField('AGEN_TEL').setReadOnly(false);
        f.findField('AGEN_ACC').setReadOnly(false);
        f.findField('UNI_NO').setReadOnly(false);
        f.findField('AGEN_BOSS').setReadOnly(false);
        f.findField('REC_STATUS').setReadOnly(false);
        f.findField('EMAIL').setReadOnly(false);
        f.findField('EMAIL_1').setReadOnly(false);
        f.findField('AGEN_BANK').setReadOnly(false);
        f.findField('AGEN_SUB').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
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
            width: 100
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 160
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 160
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 160
        }, {
            text: "效期",
            dataIndex: 'EXP_DATE',
            width: 120
        }, {
            text: "批號",
            dataIndex: 'LOT_NO',
            width: 160
        }, {
            text: "庫存數量",
            dataIndex: 'INV_QTY',
            width: 80
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                setFormT1a();
            }
        }
    });
    function setFormT1a() {
        // T1Grid.down('#edit').setDisabled(T1Rec === 0);
        // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            //var u = f.findField('AGEN_NO');
            //u.setReadOnly(true);
            //u.setFieldStyle('border: 0px');
            if (T1LastRec.data['REC_STATUS'] == 'X')
                T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
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
            xtype: 'displayfield',
            fieldLabel: '庫房代碼',
            name: 'WH_NO'
        }, {
            xtype: 'displayfield',
            fieldLabel: '院內碼',
            name: 'MMCODE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '中文品名',
            name: 'MMNAME_C'
        }, {
            xtype: 'displayfield',
            fieldLabel: '英文品名',
            name: 'MMNAME_E'
        }, {
            xtype: 'displayfield',
            fieldLabel: '效期',
            name: 'EXP_DATE'
        }, {
            xtype: 'displayfield',
            fieldLabel: '批號',
            name: 'LOT_NO'
        }, {
            xtype: 'displayfield',
            fieldLabel: '庫存數量',
            name: 'INV_QTY'
        }
        ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                    else
                    {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        }
                        );
                    }
                }
                else
                {
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
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
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
        }
    }
    function T1Cleanup() {
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
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
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
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
    T1Query.getForm().findField('P1').setValue(6);
    //var combo = T1Query.getForm().findField('P0');
    //combo.select(combo.getStore().getAt(0));    // 若有資料combo選取第一筆資料
});
