Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "品項條碼對照維護";

    var T1Rec = 0;
    var T1LastRec = null;
    var BARCODE_OLD = "";

    var GetCLSNAME = '/api/CB0002/GetCLSNAME';
    var MmcodeComboGet = '/api/CB0002/GetMmcodeCombo';
    var reportUrl = '/Report/C/CB0002.aspx';

    // 院內碼類別清單
    var MmcodeQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });

    var CLSNAMEStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });

    var STATUS_DStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "", "TEXT": "全部" },
            { "VALUE": "Y", "TEXT": "Y 使用中" },
            { "VALUE": "N", "TEXT": "N 停用" }
        ]
    });
    var mat_class = "";
    function SetCLSNAME() { //建立物料分類的下拉式選單
        Ext.Ajax.request({
            url: GetCLSNAME,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var clsnames = data.etts;
                    if (clsnames.length > 0) {
                        for (var i = 0; i < clsnames.length; i++) {
                            CLSNAMEStore.add({ VALUE: clsnames[i].VALUE, TEXT: clsnames[i].TEXT });
                        }
                        T1Query.getForm().findField("P0").setValue(clsnames[0].VALUE);
                        mat_class = clsnames[0].VALUE;
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    SetCLSNAME();

    //var Q1FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
    //    name: 'qryMMCODE',
    //    fieldLabel: '院內碼',
    //    limit: 20,
    //    queryUrl: MmcodeComboGet,
    //    storeAutoLoad: true,
    //    insertEmptyRow: true,
    //    listeners: {
    //        focus: function (field, event, eOpts) {
    //            if (!field.isExpanded) {
    //                setTimeout(function () {
    //                    field.expand();
    //                }, 300);
    //            }
    //        }
    //    }
    //});
    var T1FormMmcode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        limit: 20,
        queryUrl: MmcodeComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        allowBlank: false,
        fieldCls: 'required',
        readOnly: true,
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: mat_class
            };
        },
        listeners: {
            change: function (combo, newValue, oldValue, eOpts) {
                Ext.Ajax.request({
                    url: '/api/CB0002/GetMmcodeData',
                    method: reqVal_p,
                    params: {
                        mmcode: newValue
                    },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            var d = data.etts;
                            if (d.length > 0) {
                                T1Form.getForm().findField("MMNAME_C").setValue(data.etts[0].MMNAME_C);
                                T1Form.getForm().findField("MMNAME_E").setValue(data.etts[0].MMNAME_E);
                            }
                        }
                    },
                    failure: function (response, options) {
                    }
                });
            }
        }
    });

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 60,
            width: 160
        },
        items: [{
            xtype: 'container',
            //layout: {
            //    type: 'table',
            //    columns: 12,
            //    border: false,
            //    tdAttrs: { valign: 'top' }
            //},
            plugins: 'responsive',
            responsiveConfig: {
                'width < 600': {
                    layout: {
                        type: 'box',
                        vertical: true,
                        align: 'stretch'
                    }
                },
                'width >= 600': {
                    layout: {
                        type: 'box',
                        vertical: false
                    }
                }
            },
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'box',
                                vertical: true,
                                align: 'stretch'
                            }
                        },
                        'width >= 600': {
                            type: 'box',
                            vertical: false
                        }
                    },
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '物料分類',
                            name: 'P0',
                            id: 'P0',
                            store: CLSNAMEStore,
                            matchFieldWidth: true,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            autoSelect: true,
                            anyMatch: true,
                            labelWidth: 60,
                            width: 180,
                            listeners: {
                                focus: function (field, event, eOpts) {
                                    if (!field.isExpanded) {
                                        setTimeout(function () {
                                            field.expand();
                                        }, 300);
                                    }
                                },
                                change: function (model, records) {
                                    mat_class = records;
                                }
                            }
                        }, //Q1FormMmcode
                        {
                            xtype: 'textfield',
                            fieldLabel: '中文品名',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 200 // 可輸入最大長度為200
                        }]
                },
                {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'box',
                                vertical: true,
                                align: 'stretch'
                            }
                        },
                        'width >= 600': {
                            type: 'box',
                            vertical: false
                        }
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'P1',
                            id: 'P1',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 13
                        }, {
                            xtype: 'textfield',
                            fieldLabel: '英文品名',
                            name: 'P5',
                            id: 'P5',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 200 // 可輸入最大長度為200
                        }]
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'box',
                                vertical: true,
                                align: 'stretch'
                            }
                        },
                        'width >= 600': {
                            type: 'box',
                            vertical: false
                        }
                    },
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '品項條碼',
                            name: 'P4',
                            id: 'P4',
                            enforceMaxLength: true, // 限制可輸入最大長度
                            maxLength: 200 // 可輸入最大長度為200
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '條碼狀態',
                            name: 'P3',
                            id: 'P3',
                            padding: '0 2 0 2',
                            store: STATUS_DStore,
                            queryMode: 'local',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            value: '',
                            autoSelect: true,
                            anyMatch: true,
                            labelWidth: 60,
                            width: 140,
                            listeners: {
                                focus: function (field, event, eOpts) {
                                    if (!field.isExpanded) {
                                        setTimeout(function () {
                                            field.expand();
                                        }, 300);
                                    }
                                }
                            }
                        }]
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            layout: {
                                type: 'box',
                                vertical: true,
                                align: 'stretch'
                            }
                        },
                        'width >= 600': {
                            type: 'box',
                            vertical: false
                        }
                    },
                    items: [
                        {
                            xtype: 'button',
                            itemId: 'query', text: '查詢',
                            padding: '2 4 2 4',
                            handler: function () {
                                msglabel("");
                                T1Load();
                            }
                        }, {
                            xtype: 'button',
                            padding: '2 4 2 4',
                            itemId: 'clean', text: '清除', handler: function () {
                                var f = this.up('form').getForm();
                                T1Query.getForm().findField("P1").setValue("");
                                T1Query.getForm().findField("P2").setValue("");
                                T1Query.getForm().findField("P4").setValue("");
                                T1Query.getForm().findField("P5").setValue("");
                            }
                        }]
                }, {
                    xtype: 'panel',
                    border: false,
                    margin: '1 0 1 0',
                    itemId: 'queryEditButtons',
                    // hidden: true,
                    plugins: 'responsive',
                    responsiveConfig: {
                        'width < 600': {
                            hidden: false,
                            layout: {
                                type: 'box'
                            }
                        },
                        'width >= 600': {
                            hidden: true
                            //layout: {
                            //    type: 'box',
                            //    vertical: false
                            //}
                        }
                    },
                    items: [{
                        xtype: 'button',
                        text: '新增',
                        id: 'QbtnAdd',
                        name: 'btnAdd',
                        handler: function () {
                            setFormT1('I', '新增');
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'Qedit',
                        text: '修改',
                        disabled: true,
                        handler: function () {
                            setFormT1("U", '修改');
                        }
                    }, {
                        xtype: 'button',
                        itemId: 'Qdelete',
                        text: '刪除',
                        disabled: true,
                        handler: function () {
                            setFormT1("D", '刪除');
                        }
                    }
                    //  , {
                    //    xtype: 'hiddenfield',
                    //    itemId: 'Qt1print',
                    //    text: '列印',
                    //    disabled: false,
                    //    handler: function () {
                    //        showReport();
                    //    }
                    //}
                    ]
                }
            ]
        }

        ]
    });
    var T1Store = Ext.create('WEBAPP.store.CB0002M', { // 定義於/Scripts/app/store/CB0002M.js
        listeners: {
            beforeload: function (store, options) {
                T1Grid.deselectAll;
                T1Form.reset();
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue()
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
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                },
                handler: function () {
                    setFormT1('I', '新增');
                }
            }, {
                itemId: 'edit',
                text: '修改',
                disabled: true,
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                },
                handler: function () {
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 'delete',
                text: '刪除',
                disabled: true,
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                },
                handler: function () {
                    setFormT1("D", '刪除');
                }
            }, {
                itemId: 'export', text: '匯出', disabled: false,
                plugins: 'responsive',
                responsiveConfig: {
                    'width < 600': {
                        hidden: true
                    },
                    'width >= 600': {
                        hidden: false
                    }
                },
                handler: function () {
                    Ext.MessageBox.confirm('匯出', '是否確定匯出？', function (btn, text) {
                        if (btn === 'yes') {
                            var p = new Array();
                            //MAT_CLASS, MMCODE, MMNAME_C, STATUS, BARCODE, MMNAME_E
                            p.push({ name: 'MAT_CLASS', value: T1Query.getForm().findField('P0').getValue() }); //SQL篩選條件
                            p.push({ name: 'MMCODE', value: T1Query.getForm().findField('P1').getValue() }); //SQL篩選條件
                            p.push({ name: 'MMNAME_C', value: T1Query.getForm().findField('P2').getValue() }); //SQL篩選條件
                            p.push({ name: 'STATUS', value: T1Query.getForm().findField('P3').getValue() }); //SQL篩選條件
                            p.push({ name: 'BARCODE', value: T1Query.getForm().findField('P4').getValue() }); //SQL篩選條件
                            p.push({ name: 'MMNAME_E', value: T1Query.getForm().findField('P5').getValue() }); //SQL篩選條件
                            PostForm('/api/CB0002/Excel', p);
                        }
                    });
                }
            }
            //, {
            //    itemId: 't1print',
            //    text: '列印',
            //    disabled: false,
            //    plugins: 'responsive',
            //    responsiveConfig: {
            //        'width < 600': {
            //            hidden: true
            //        },
            //        'width >= 600': {
            //            hidden: false
            //        }
            //    },
            //    hidden: true,
            //    handler: function () {
            //        showReport();
            //    }
            //}
        ]
    });

    function setFormT1(x, t) {
        //viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField('x').setValue(x);

        f.getFields().each(function (fc) {
            fc.setReadOnly(false);
        });
        if (x == "I") {
            T1Set = '/api/CB0002/Create';
            f.findField('MMCODE').setReadOnly(false);
            if (T1LastRec) {
                f.reset();
                f.findField('BARCODE').setValue('');
                f.findField('TRATIO').setValue('1');
            }
            T1Form.down('#t1cancel').show();
            T1Form.down('#t1submit').show();
        } else if ("R" === x) {
            f.getFields().each(function (fc) {
                fc.setReadOnly(true);
            });
            T1Form.down('#t1cancel').hide();
            T1Form.down('#t1submit').hide();
        }
        if (x == "U") {
            T1Set = '/api/CB0002/Update';
            if (T1LastRec) {
                f.loadRecord(T1LastRec);
                f.findField('BARCODE_OLD').setValue(f.findField('BARCODE').getValue());
                f.findField('MMCODE').setReadOnly(true);
                f.findField('BARCODE').setReadOnly(false);
            }
            T1Form.getForm().findField('BARCODE').focus();
            T1Form.down('#t1cancel').show();
            T1Form.down('#t1submit').show();
        }
        if (x == "D") {
            T1Set = '/api/CB0002/Delete';
            if (T1LastRec) {
                f.loadRecord(T1LastRec);
                f.findField('BARCODE_OLD').setValue(f.findField('BARCODE').getValue());
                f.findField('MMCODE').setReadOnly(true);
                f.findField('BARCODE').setReadOnly(false);
                if (f.findField('MMCODE').getValue() == f.findField('BARCODE').getValue()) {
                    Ext.Msg.alert('提醒', '此筆<font color=blue>【院內碼】=【條碼】</font>不能刪除!');
                } else {
                    Ext.MessageBox.confirm('提醒', '確定刪除條碼<font color=blue>' + f.findField('BARCODE_OLD').getValue() + '</font>?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    }
                    );
                }
            }
        }
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
            xtype: 'rownumberer',
            text: '項次',
            width: 50
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS',
            width: 70
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 150
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 180
        }, {
            text: "條碼",
            dataIndex: 'BARCODE',
            width: 130
        }, {
            text: "使用代碼",
            dataIndex: 'STATUS',
            width: 100,
            renderer: function (value) {
                if (value == 'Y')
                { return '使用中'; }
                if (value == 'N')
                { return '停用'; }
            }
        }, {
            text: "轉換率",
            dataIndex: 'TRATIO',
            width: 60
        },
        {
            header: "",
            flex: 1
        }],
        viewConfig: {
            listeners: {
                refresh: function (view) {
                    T1Grid.down('#export').setDisabled(true);
                    //T1Grid.down('#t1print').setDisabled(true);
                    //T1Query.down('#Qt1print').setDisabled(true);
                    var allRecords = T1Store.data;
                    //有設定儲位條碼則可列印
                    allRecords.each(function (record) {
                        if (record.data.BARCODE.toString() != '') {
                            T1Grid.down('#export').setDisabled(false);
                            //T1Grid.down('#t1print').setDisabled(false);
                            //T1Query.down('#Qt1print').setDisabled(false);
                            return false;
                        }
                    });
                }
            }
        },
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                T1Grid.down('#edit').setDisabled(false);
                T1Query.down('#Qedit').setDisabled(false);
                T1Grid.down('#delete').setDisabled(false);
                T1Grid.down('#Qdelete').setDisabled(false);
                setFormT1("R", '瀏覽');
                if (T1LastRec) {
                    T1Form.getForm().loadRecord(T1LastRec);
                }
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec == 0);
        T1Grid.down('#delete').setDisabled(T1Rec == 0);
        T1Query.down('#Qedit').setDisabled(T1Rec == 0);
        T1Query.down('#Qdelete').setDisabled(T1Rec == 0);
        if (T1LastRec) {
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
        },
        {
            name: 'BARCODE_OLD',
            xtype: 'hidden'
        },
        {
            xtype: 'displayfield',
            fieldLabel: '物料分類',
            name: 'MAT_CLASS'
        }, T1FormMmcode,
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
            fieldLabel: '條碼',
            id: 'BARCODE',
            name: 'BARCODE',
            enforceMaxLength: true,
            maxLength: 200,
            readOnly: true,
            allowBlank: false,
            fieldCls: 'required'

        }, {
            xtype: 'textfield',
            fieldLabel: '轉換率',
            id: 'TRATIO',
            name: 'TRATIO',
            queryMode: 'local',
            matchFieldWidth: true,
            blankText: "請輸入[轉換率]",
            maskRe: /[0-9]/,
            regexText: '只能輸入數字',
            regex: /^([1-9][0-9]*|0)$/
        }, {
            xtype: 'combo',
            fieldLabel: '使用代碼',
            name: 'STATUS',
            queryMode: 'local',
            displayField: 'name',
            valueField: 'abbr',
            autoSelect: true,
            multiSelect: false,
            editable: false, typeAhead: true, forceSelection: true, selectOnFocus: true,
            matchFieldWidth: true,
            blankText: "請選擇[使用代碼]",
            readOnly: true,
            value: 'Y',
            store: [
                { abbr: 'Y', name: '使用中' },
                { abbr: 'N', name: '停用' }
            ]
        }],
        buttons: [{
            itemId: 't1submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    //var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                    T1Submit();
                    //Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                    //    if (btn === 'yes') {
                    //        T1Submit();
                    //    }
                    //}
                    //);
                }
                else {
                    var str = '';

                    Ext.Msg.alert('提醒', '輸入資料格式有誤' + str);
                }
            }
        }, {
            itemId: 't1cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
    });
    function T1Submit() {
        var f = T1Form.getForm();
        var vP0 = T1Query.getForm().findField('P0').getValue();
        var vP1 = T1Query.getForm().findField('P1').getValue();
        var vP2 = T1Query.getForm().findField('P2').getValue();
        var vP3 = T1Query.getForm().findField('P3').getValue();
        var vP4 = T1Query.getForm().findField('P4').getValue();
        var vP5 = T1Query.getForm().findField('P5').getValue();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                params: {
                },
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel("條碼資料新增成功");
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.MAT_CLASS_CODE);
                            T1Query.getForm().findField('P1').setValue(v.MMCODE);
                            T1Query.getForm().findField('P2').setValue('');
                            T1Query.getForm().findField('P3').setValue('');
                            T1Query.getForm().findField('P4').setValue('');
                            T1Query.getForm().findField('P5').setValue('');
                            T1Load();
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel("條碼資料更新完成");
                            T1Grid.getView().refresh();
                            break;
                        case "D":
                            if (action.result.msg == "") {
                                msglabel("條碼資料刪完成");
                                T1Query.getForm().findField('P0').setValue(vP0);
                                T1Query.getForm().findField('P1').setValue(vP1);
                                T1Query.getForm().findField('P2').setValue(vP2);
                                T1Query.getForm().findField('P3').setValue(vP3);
                                T1Query.getForm().findField('P4').setValue(vP4);
                                T1Query.getForm().findField('P5').setValue(vP5);
                                T1Load();
                            } else {
                                Ext.Msg.alert('提示', action.result.msg);
                                msglabel(action.result.msg);
                            }

                            break;
                    }
                    T1Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', '輸入資料有誤');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', '連線錯誤');
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
        viewport.down('#form').setTitle('瀏覽');
        T1Grid.getSelectionModel().deselectAll();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            fc.setReadOnly(true);
        });
        T1Form.down('#t1cancel').hide();
        T1Form.down('#t1submit').hide();
        setFormT1a();
        T1Grid.down('#edit').setDisabled(T1Rec == 0);
        T1Grid.down('#delete').setDisabled(T1Rec == 0);
        T1Query.down('#Qedit').setDisabled(T1Rec == 0);
        T1Query.down('#Qdelete').setDisabled(T1Rec == 0);
        viewport.down('#form').collapse();
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?MMCODE=' + T1Query.getForm().findField('P1').getValue()
                + '&MMNAME_C=' + T1Query.getForm().findField('P2').getValue()
                + '&MMNAME_E=' + T1Query.getForm().findField('P5').getValue()
                + '&BARCODE=' + T1Query.getForm().findField('P4').getValue()
                + '&STATUS=' + T1Query.getForm().findField('P3').getValue()
                + '&MAT_CLASS=' + T1Query.getForm().findField('P0').getValue()
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    margin: '0 20 30 0',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 200, viewport.height - 20);
        }
        win.show();
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
            id: 'eastform',
            region: 'east',
            collapsible: true,
            collapsed: true,
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
});
