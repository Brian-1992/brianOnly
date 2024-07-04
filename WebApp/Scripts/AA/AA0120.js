Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BE0002/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "計量單位轉換率檔維護";

    var T1Rec = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

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
                    xtype: 'textfield',
                    fieldLabel: '院內碼',
                    labelWidth: 60,
                    width: 180,
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 13, // 可輸入最大長度為13
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '計量單位代碼',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 6,
                    width: 160,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'textfield',
                    fieldLabel: '廠商代碼',
                    labelWidth: 60,
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    maxLength: 6,
                    width: 130,
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
                },
                {
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

    //var T1Store = Ext.create('WEBAPP.store.MiUnitexch', { // 定義於/Scripts/app/store/MiUnitexch.js
    //    listeners: {
    //        beforeload: function (store, options) {
    //            // 載入前將查詢條件P0~P4的值代入參數
    //            var np = {
    //                p0: T1Query.getForm().findField('P0').getValue(),
    //                p1: T1Query.getForm().findField('P1').getValue(),
    //                p2: T1Query.getForm().findField('P2').getValue()
    //            };
    //            Ext.apply(store.proxy.extraParams, np);
    //        }
    //    }
    //});
    //function T1Load() {
    //    T1Store.load({
    //        params: {
    //            start: 0
    //        }
    //    });
    //}

    var viewModel = Ext.create('WEBAPP.store.AA.AA0120VM');
    var T1Store = viewModel.getStore('MI_UNITEXCH');
    function T1Load() {
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    //var T2Store = viewModel.getStore('MI_MAST');
    //function T2Load() {
    //    T2Store.load({
    //        params: {
    //            start: 0
    //        }
    //    });
    //}

    var T3Store = viewModel.getStore('MI_UNITCODE');
    function T3Load() {
        T3Store.load({
            params: {
                start: 0
            }
        });
    }

    var T4Store = viewModel.getStore('PH_VENDOR');
    //T4Store.getProxy().setUrl('/api/AA0120/GetPhVendor'); 
    function T4Load() {
        T4Store.load({
            params: {
                start: 0
            }
        });
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
                    T1Set = '/api/AA0120/Create'; // AA0120Controller的Create
                    setFormT1('I', '新增');
                    //T2Load();
                    T3Load();
                    T4Load();
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/AA0120/Update';
                    setFormT1("U", '修改');
                    //T2Load();
                    T3Load();
                    T4Load();
                }
            }
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
        var u;
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.MiUnitexch'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('MMCODE').setReadOnly(false);
            f.findField('UNIT_CODE').setReadOnly(false);
            f.findField('AGEN_NO').setReadOnly(false);
            u = f.findField("EXCH_RATIO");
            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
        } else if (x === 'U') {
            f.findField('MMCODE').setReadOnly(true);
            f.findField('UNIT_CODE').setReadOnly(true);
            f.findField('AGEN_NO').setReadOnly(true);
        }
        else {
            u = f.findField('AGEN_NO');
        }
        f.findField('x').setValue(x);
        f.findField('EXCH_RATIO').setReadOnly(false);;
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
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "院內碼中文名",
                dataIndex: 'MMNAME_C',
                width: 160
            },
            {
                text: "計量單位代碼",
                dataIndex: 'UNIT_CODE',
                width: 100
            },
            {
                text: "計量單位",
                dataIndex: 'UI_CHANAME',
                width: 70
            },
            {
                text: "廠商代碼",
                dataIndex: 'AGEN_NO',
                width: 100
            },
            {
                text: "廠商名",
                dataIndex: 'AGEN_NAMEC',
                width: 160
            },
            {
                text: "轉換率",
                dataIndex: 'EXCH_RATIO',
                width: 70
            },
            {
                header: "",
                flex: 1
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
        // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('AGEN_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            //if (T1LastRec.data['REC_STATUS'] == 'X')
            //    T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
        }
        else {
            T1Form.getForm().reset();
        }
    }

    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,
        readOnly: true,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0120/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        //getDefaultParams: function () {
        //    var f = T1Form.getForm();
        //    if (!f.findField("MMCODE").readOnly) {
        //        var tmpArray = f.findField("MAT_CLASS2").getValue().split(' ');
        //        var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
        //        return {
        //            MAT_CLASS: tmpArray[0],
        //            WH_NO: tmpArray2[0]
        //        };
        //    }
        //},
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    //f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    //f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    //f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                }
            }
        }
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
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            mmCodeCombo,
            //{
            //    xtype: 'combo',
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    store: T2Store,
            //    displayField: 'MMNAME',
            //    valueField: 'MMCODE',
            //    anyMatch: true,
            //    readOnly: true,
            //    allowBlank: false, // 欄位為必填
            //    typeAhead: true,
            //    forceSelection: true,
            //    queryMode: 'local',
            //    triggerAction: 'all',
            //    fieldCls: 'required'
            //}, 
            {
                //fieldLabel: '計量單位代碼',
                //name: 'UNIT_CODE',
                //enforceMaxLength: true,
                //maxLength: 6,
                //readOnly: true,
                //allowBlank: false, // 欄位為必填
                //fieldCls: 'required'
                xtype: 'combo',
                fieldLabel: '計量單位代碼',
                name: 'UNIT_CODE',
                store: T3Store,
                displayField: 'UI_NAME',
                valueField: 'UNIT_CODE',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                //fieldLabel: '廠商代碼',
                //name: 'AGEN_NO',
                //enforceMaxLength: true,
                //maxLength: 6,
                //readOnly: true,
                //allowBlank: false, // 欄位為必填
                //fieldCls: 'required'
                xtype: 'combo',
                fieldLabel: '廠商代碼',
                name: 'AGEN_NO',
                store: T4Store,
                displayField: 'AGEN_NAMEC',
                valueField: 'AGEN_NO',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{AGEN_NAMEC}&nbsp;</div></tpl>',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                fieldLabel: '轉換率',
                name: 'EXCH_RATIO',
                enforceMaxLength: true,
                maxLength: 10,
                readOnly: true
            }],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                    //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                    //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                    //else {
                    //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    //        if (btn === 'yes') {
                    //            T1Submit();
                    //        }
                    //    });
                    //}

                    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });
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
                    var v;
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            v = action.result.etts[0];
                            f.clearInvalid();
                            T1Query.getForm().findField('P0').setValue(v.MMCODE);
                            T1Query.getForm().findField('P1').setValue(v.UNIT_CODE);
                            T1Query.getForm().findField('P2').setValue(v.AGEN_NO);
                            T1Load();
                            break;
                        case "U":
                            v = action.result.etts[0];
                            r.set(v);
                            r.commit();
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
            fc.setReadOnly(true);
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
    //T2Load();
    T3Load();
    T4Load();
    T1Query.getForm().findField('P0').focus();
});
