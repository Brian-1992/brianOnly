Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var WhnoComboGet = '../../../api/CE0002/GetWhnoCombo';
    var T1Name = '初盤管理作業';
    var reportUrl = '/Report/C/CE0002.aspx';
    var reportUrlPH1S = '/Report/C/CE0002_1.aspx';
    var reportUrlWard = '/Report/C/CE0002_2.aspx';
    var reportMultiChknoUrl = '/Report/C/CE0016.aspx';
    var reportMultiMmcodeUrl = '/Report/C/CE0016_1.aspx';
    var reportMultiMmcodeWardUrl = '/Report/C/CE0016_2.aspx';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];
    var windowHeight = $(window).height();
    //var userId, userName, userInid, userInidName;
    var T1cell = '';
    var T1LastRec = null;
    var loadT22 = false;
    var chkPeriod = 'D';
    var orderway = 'STORE_LOC';
    var is_distri = 'false';
    var chkClass = '';
    var windowNewOpen = true;
    var todayDateString = '';
    var temp_mast = {};
    var currentSetYm = '';

    var viewModel = Ext.create('WEBAPP.store.CE0002VM');

    // 庫房清單
    var whnoQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    var whnoCreateStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: WhnoComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            whnoQueryStore.add(wh_nos[i]);
                            
                            if (!(wh_nos[i].WH_GRADE == '2' && wh_nos[i].WH_KIND == '0')) {
                                whnoCreateStore.add(wh_nos[i]);
                            }
                        }
                        var f = T1Form.getForm();

                        f.findField('CHK_WH_NO').setValue(wh_nos[0].WH_NO);
                        f.findField('CHK_WH_GRADE').setValue(wh_nos[0].WH_GRADE);
                        f.findField('CHK_WH_KIND').setValue(wh_nos[0].WH_KIND);
                        changeChkClass('');

                        T1Query.getForm().findField('P0').setValue(wh_nos[0].WH_NO);
                        T1Load(true);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    //setComboData();

    function getTodayDate() {
        Ext.Ajax.request({
            url: '/api/CE0002/CurrentDate',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    todayDateString = data.msg;
                    setComboData();
                }
            },
            failure: function (response, options) {

            }
        });
    }
    getTodayDate();

    var T1Store = viewModel.getStore('MasterAll');
    function T1Load(clearMsg) {
        //
        if (clearMsg) {
            msglabel('');
        }

        //T1LastRec = null;

        var p0 = T1Query.getForm().findField('P0').getValue();
        if (p0 == null) {
            p0 = "";
        }

        T1Store.getProxy().setExtraParam("p0", p0);
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').rawValue);
        T1Store.getProxy().setExtraParam("p2", userId);
        T1Tool.moveFirst();


        Ext.getCmp('btnMedChangeUid').hide();
        Ext.getCmp('btnWardChangeUid').hide();
        Ext.getCmp('btnInvqty0Confirm').hide();
        var whItem = whnoQueryStore.findRecord('WH_NO', p0);
        if (whItem.get('WH_GRADE') == '1') {
            if (whItem.get('WH_KIND') == '0') {
                Ext.getCmp('btnInvqty0Confirm').show();
            } else {
                return;
            }
        } else if (whItem.get('WH_KIND') == '0' && whItem.get('WH_GRADE') == "2") {
            Ext.getCmp('btnMedChangeUid').show();
        } else {
            Ext.getCmp('btnWardChangeUid').show();
        }

        if (p0 != '') {
            checkNeedDetailAdd(p0);
        }
    }

    function getColumnIndex(columns, dataIndex) {
        var index = -1;
        for (var i = 0; i < columns.length; i++) {
            if (columns[i].dataIndex == dataIndex) {
                index = i;
            }
        }

        return index;
    }

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
            xtype: 'container',
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    width: '100%',
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'combo',
                            store: whnoQueryStore,
                            name: 'P0',
                            id: 'P0',
                            fieldLabel: '庫房代碼',
                            displayField: 'WH_NAME',
                            valueField: 'WH_NO',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            fieldCls: 'required',
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                        },
                        {
                            xtype: 'monthfield',
                            fieldLabel: '盤點年月',
                            name: 'P1',
                            id: 'P1',
                            // enforceMaxLength: true,
                            //maxLength: 5,
                            //minLength: 5,
                            //regexText: '請填入民國年月',
                            //regex: /\d{5,5}/,
                            labelWidth: mLabelWidth,
                            width: 180,
                            padding: '0 4 0 4',
                            //format: 'Xm',
                            value: new Date()
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '負責人',
                            name: 'P2',
                            id: 'P2',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 210,
                            padding: '0 4 0 4',
                            value: userName
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T1Query.getForm();
                                if (!f.findField('P0').getValue()) {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房代碼</span>為必填');
                                    return;
                                }

                                T1Load(true);

                                Ext.getCmp('eastform').collapse();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }, {
                            xtype: 'button',
                            text: '新增開單後庫存異動品項',
                            id:'addNotExists',
                            handler: function () {
                                var f = this.up('form').getForm();
                                Ext.MessageBox.confirm('', '品項新增至本庫房<span style="color:red">開立</span>或<span style="color:red">盤中</span>之盤點單<br/>是否確定？', function (btn, text) {
                                    if (btn === 'yes') {
                                        addNotExists(T1Query.getForm().findField('P0').getValue());
                                    }
                                }
                                );
                            }
                        },
                        {
                            xtype: 'component',
                            flex: 1
                        },
                        {
                            xtype: 'container',
                            layout: 'hbox',
                            right: '100%',
                            items: [
                                {
                                    xtype: 'button',
                                    text: '前往初盤數量輸入(電腦版)',
                                    handler: function () {
                                        parent.link2('/Form/Index/CE0016', ' 初盤數量輸入作業(CE0016)', true);
                                        //parent.link2('/Form/Index/CE0016','初盤數量輸入作業');
                                    }
                                }
                            ]
                        }
                    ]
                }
            ]
        }]
    });
    var add = function () {
        setFormT1('I', '新增');

        var f = T1Form.getForm();

        f.findField('CHK_WH_KIND_P').hide();

        Ext.getCmp('chk_period_d').show();
        Ext.getCmp('chk_period_m').show();


        var queryWhno = whnoQueryStore.findRecord('WH_NO', T1Query.getForm().findField('P0').getValue());

        f.findField('CHK_WH_NO').setValue(queryWhno.get('WH_NO'));
        f.findField('CHK_WH_KIND').setValue(queryWhno.get('WH_KIND'));
        f.findField('CHK_WH_GRADE').setValue(queryWhno.get('WH_GRADE'));


        if (f.findField('CHK_WH_KIND').getValue() == '0') {     // 藥品庫


            f.findField('CHK_WH_KIND_0').show();
            f.findField('CHK_WH_KIND_1').hide();
            f.findField('CHK_CLASS').hide();
            f.findField('CHK_CLASS_E').hide();
            f.findField('CHK_CLASS_C').hide();
            f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '1' });

            if (f.findField('CHK_WH_GRADE').getValue() == "2") {
                Ext.getCmp('chk_period_d').hide();
                Ext.getCmp('chk_period_m').hide();
            } else {
                Ext.getCmp('chk_period_s').hide();
                Ext.getCmp('chk_period_p').hide();
            }

            Ext.getCmp('chk_wh_kind0_1').show();
            Ext.getCmp('chk_wh_kind0_2').show();
            Ext.getCmp('chk_wh_kind0_5').hide();
            Ext.getCmp('chk_wh_kind0_6').hide();
            Ext.getCmp('chk_wh_kind0_7').hide();
            Ext.getCmp('chk_wh_kind0_8').hide();
            if (f.findField('CHK_WH_NO').getValue() == "ANE1") {   // 麻醉科
                //
                Ext.getCmp('chk_wh_kind0_1').hide();
                Ext.getCmp('chk_wh_kind0_2').hide();
                Ext.getCmp('chk_wh_kind0_5').show();
                Ext.getCmp('chk_wh_kind0_6').show();
                Ext.getCmp('chk_wh_kind0_7').hide();
                Ext.getCmp('chk_wh_kind0_8').hide();
                f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '5' });
            } else if (Number(f.findField('CHK_WH_GRADE').getValue()) > 2) {
                Ext.getCmp('chk_wh_kind0_1').hide();
                Ext.getCmp('chk_wh_kind0_2').hide();
                Ext.getCmp('chk_wh_kind0_5').hide();
                Ext.getCmp('chk_wh_kind0_6').hide();
                Ext.getCmp('chk_wh_kind0_7').show();
                Ext.getCmp('chk_wh_kind0_8').show();
                f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '7' });
            }

        } else if (f.findField('CHK_WH_KIND').getValue() == '1') {   // 衛材庫
            f.findField('CHK_WH_KIND_1').show();
            f.findField('CHK_WH_KIND_0').hide();
            Ext.getCmp('chk_period_s').hide();
            Ext.getCmp('chk_period_p').hide();
            Ext.getCmp('chk_period_m').hide();
            f.findField('CHK_CLASS').hide();
            f.findField('CHK_CLASS_E').hide();
            f.findField('CHK_CLASS_C').hide();
            f.findField('CHK_WH_KIND_1').setValue({ CHK_WH_KIND_1: '1' });
            Ext.getCmp('chk_wh_kind1_0').show();
            Ext.getCmp('chk_wh_kind1_2').show();

            Ext.getCmp('chk_period_m').setValue(false);
            Ext.getCmp('chk_period_d').setValue(true);
            changeChkPeriod('D');

            if (f.findField('CHK_WH_GRADE').getValue() == "1") {
                f.findField('CHK_CLASS').show();
                f.findField('CHK_CLASS').setValue({ CHK_CLASS: '02' });
                changeChkClass('02');
                Ext.getCmp('chk_period_m').show();
                Ext.getCmp('chk_period_d').hide();
                Ext.getCmp('chk_period_m').setValue(true);
                changeChkPeriod('M');

            }
            else {
                changeChkClass('02');
                Ext.getCmp('chk_period_m').hide();
                Ext.getCmp('chk_period_d').show();
            }


        } else if (f.findField('CHK_WH_KIND').getValue() == 'E') {   // 能設                              
            f.findField('CHK_WH_KIND_1').show();
            f.findField('CHK_WH_KIND_0').hide();

            Ext.getCmp('chk_period_s').hide();
            Ext.getCmp('chk_period_p').hide();
            Ext.getCmp('chk_period_m').show();
            Ext.getCmp('chk_period_d').hide();
            Ext.getCmp('chk_period_m').setValue(true);
            changeChkPeriod('M');

            f.findField('CHK_CLASS_E').show();
            f.findField('CHK_CLASS_C').hide();
            f.findField('CHK_CLASS').hide();
            f.findField('CHK_CLASS_E').setValue({ CHK_CLASS_E: '14' });
            Ext.getCmp('chk_wh_kind1_0').hide();
            Ext.getCmp('chk_wh_kind1_2').hide();
            Ext.getCmp('chk_wh_kind1_1').setValue(true);
            changeChkClass('14');

        } else if (f.findField('CHK_WH_KIND').getValue() == 'C') {   // 通信                           
            f.findField('CHK_WH_KIND_1').show();
            f.findField('CHK_WH_KIND_0').hide();

            Ext.getCmp('chk_period_s').hide();
            Ext.getCmp('chk_period_p').hide();
            Ext.getCmp('chk_period_m').show();
            Ext.getCmp('chk_period_d').hide();
            Ext.getCmp('chk_period_m').setValue(true);
            changeChkPeriod('M');

            f.findField('CHK_CLASS_E').hide();
            f.findField('CHK_CLASS_C').show();
            f.findField('CHK_CLASS').hide();
            f.findField('CHK_CLASS_C').setValue({ CHK_CLASS_C: '30' });
            Ext.getCmp('chk_wh_kind1_0').hide();
            Ext.getCmp('chk_wh_kind1_2').hide();
            Ext.getCmp('chk_wh_kind1_1').setValue(true);
            changeChkClass('30');

        }
        changeChkPeriod(chkPeriod);
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    
                    add();


                }
            },
            {
                text: '刪除',
                id: 'btnDelete',
                name: 'btnDelete',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要刪除的盤點單');
                        return;
                    }

                    for (var i = 0; i < selection.length; i++) {
                        if (Number(selection[i].data.CHK_STATUS) > 1 || isNaN(Number(selection[i].data.CHK_STATUS))) {
                            var msg = '盤點單號' + selection[i].data.CHK_NO + '不可刪除，請重新選擇';
                            Ext.Msg.alert('提醒', msg);
                            return;
                        }
                    }

                    var list = [];
                    for (var i = 0; i < selection.length; i++) {
                        list.push(selection[i].data);
                    }
                    //var r = WEBAPP.model.ME_DOCM.create({
                    //    //APPNAME: 'aaa'
                    //});
                    //T1Store.add(r.copy());
                    //T1Set = '/api/AA0035/MasterCreate';
                    //setFormT1('I', '新增');
                    Ext.MessageBox.confirm('刪除', '刪除盤點單，也會將該盤點單內的目前準備旳盤點品項刪除<br/>是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {
                            //transferToBcwhpick("");
                            deleteMaster(list);
                        }
                    }
                    );
                }
            },
            {
                itemId: 'MultiPrint',
                id: 'btnMultiPrint',
                text: "多筆列印",
                disabled: true,
                handler: function () {

                    var selection = T1Grid.getSelection();

                    //var chk_nos = "";
                    //for (var i = 0; i < selection.length; i++) {
                    //    if (i == 0) {
                    //        chk_nos = selection[i].data.CHK_NO;
                    //    } else {
                    //        chk_nos = chk_nos + "," + selection[i].data.CHK_NO;
                    //    }
                    //}
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇要列印的盤點單');
                        return;
                    }

                    for (var i = 0; i < selection.length; i++) {
                        if (Number(selection[i].data.CHK_STATUS) < 1) {
                            Ext.Msg.alert('提醒', '盤點單號：' + selection[i].data.CHK_NO + ' 狀態為準備中，不可列印');
                            return;
                        }
                    }

                    multiPrintWindow.show();
                }
            },
            {
                text: '單筆列印',
                id: 'btnPrint',
                name: 'btnPrint',
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) < 1) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 狀態為準備中，不可列印');
                        return;
                    }
                    //var items = T21Grid.getStore().data.items;
                    //detailSave();
                    T24Form.getForm().findField('print_order').setValue('store_loc, mmcode, chk_uid');
                    printWindow.show();
                }
            },
            {
                text: '修改盤點人員',
                id: 'btnMedChangeUid',
                name: 'btnMedChangeUid',
                hidden: true,
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) > 1 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 不可修改盤點人員');
                        return;
                    }

                    T5Load();
                    var title = T1LastRec.data.CHK_NO + ' ' + T1LastRec.data.WH_NAME + ' ' + T1LastRec.data.CHK_YM + ' ' + T1LastRec.data.CHK_PERIOD_NAME;
                    medChangeUidWindow.setTitle('修改盤點人員 ' + title);

                    medChangeUidWindow.show();
                }
            },
            {
                text: '修改盤點人員',
                id: 'btnWardChangeUid',
                name: 'btnWardChangeUid',
                hidden: true,
                disabled: true,
                handler: function () {
                    if (Number(T1LastRec.data.CHK_STATUS) > 1 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                        Ext.Msg.alert('提醒', '盤點單號：' + T1LastRec.data.CHK_NO + ' 不可修改盤點人員');
                        return;
                    }

                    T6Load();
                    var title = T1LastRec.data.CHK_NO + ' ' + T1LastRec.data.WH_NAME + ' ' + T1LastRec.data.CHK_YM + ' ' + T1LastRec.data.CHK_PERIOD_NAME;
                    wardChangeUidWindow.setTitle('修改盤點人員 ' + title);

                    wardChangeUidWindow.show();
                }
            },
            {
                text: '確認數量為0',
                id: 'btnInvqty0Confirm',
                name: 'btnInvqty0Confirm',
                hidden: true,
                disabled: true,
                handler: function () {
                    
                    var selection = T1Grid.getSelection();
                    var chk_nos = '';
                    for (var i = 0; i < selection.length; i++) {
                        if (selection[i].data.CHK_STATUS != '1') {
                            var msg = selection[i].data.CHK_NO + ' 不可確認數量為0，請重新選擇';
                            Ext.Msg.alert('提醒', msg);
                            return;
                        }

                        if (chk_nos != '') {
                            chk_nos += ',';
                        }
                        chk_nos += ("'" + selection[i].data.CHK_NO + "'");
                    }

                    Ext.MessageBox.confirm('確認', '是否確認電腦量為0的品項盤點量也為0?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/CE0002/Invqty0Confirm',
                                method: reqVal_p,
                                params: {
                                    chk_nos: chk_nos
                                },
                                success: function (response) {
                                    msglabel('確認成功');
                                    T1Load(false);
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('失敗', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });
    function deleteMaster(list) {
        Ext.Ajax.request({
            url: '/api/CE0002/DeleteMaster',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(list) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("刪除成功");
                    T1Load(false);

                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }
    function copeOnWayQty(wh_no, chk_type, chk_class) {
        Ext.Ajax.request({
            url: '/api/CE0002/CopeOnWayQty',
            method: reqVal_p,
            params: {
                wh_no: wh_no,
                chk_type: chk_type, //    物料分類
                chk_class: chk_class, //    盤點類別: 庫備、非庫備、小額採購 
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel("在途量處理完成");

                    Ext.MessageBox.confirm('新增', '是否確定新增?', function (btn, text) {
                        if (btn === 'yes') {
                            T1Submit();
                        }
                    });


                } else {
                    Ext.Msg.alert('提醒', data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
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
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NAME',
                width: 120
            },
            {
                text: "盤點年月日",
                dataIndex: 'CHK_YM',
                width: 80
            },
            {
                text: "庫房級別",
                dataIndex: 'CHK_WH_GRADE',
                width: 70
            },
            //{
            //    text: "庫房分類",
            //    dataIndex: 'WH_KIND_NAME',
            //    width: 80
            //},
            {
                text: "物料分類",
                dataIndex: 'CHK_CLASS_NAME',
                width: 80
            },
            {
                text: "盤點期",
                dataIndex: 'CHK_PERIOD',
                width: 70,
                xtype: 'templatecolumn',
                tpl: '{CHK_PERIOD_NAME}'
            },
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_TYPE_NAME}'
            },
            {
                text: "盤點量/總量",
                dataIndex: 'CHK_TOTAL',
                width: 120,
                xtype: 'templatecolumn',
                tpl: '{CHK_NUM}/{CHK_TOTAL}'
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120,
                renderer: function (val, meta, record) {
                    var CHK_NO = record.data.CHK_NO;
                    return '<a href=javascript:void(0)>' + CHK_NO + '</a>';
                },
            },
            //{
            //    text: "負責人員",
            //    dataIndex: 'CHK_KEEPER',
            //    width: 120
            //},
            {
                text: "狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 80
            },
            {
                text: "已分配盤點人員",
                dataIndex: 'CHK_UID_NAMES',
                flex: 1
            },
            //{
            //    header: "",
            //    flex: 1
            //}
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {

                }
            },
            selectionchange: function (model, records) {
                //
                //msglabel('');

                //if (T1cell == '')
                //
                T1Rec = records.length;

                Ext.getCmp('btnPrint').disable();
                Ext.getCmp('btnMultiPrint').disable();
                Ext.getCmp('btnMedChangeUid').disable();
                Ext.getCmp('btnWardChangeUid').disable();
                Ext.getCmp('btnInvqty0Confirm').disable();
                Ext.getCmp('btnDelete').disable();

                if (records.length > 0) {
                    T1LastRec = Ext.clone(records[0]);

                    if (isEditable(T1Query.getForm().findField('P1').rawValue)) {
                        Ext.getCmp('btnDelete').enable();
                    } else {
                        Ext.getCmp('btnDelete').disable();
                    }
                }

                for (var i = 0; i < records.length; i++) {

                    if (records[i].data.CREATE_USER == 'BATCH' || records[i].data.CREATE_USER == 'CE0015') {
                        Ext.getCmp('btnDelete').disable();
                    }
                }

                if (records.length == 1) {
                    Ext.getCmp('btnPrint').enable();
                    Ext.getCmp('btnMultiPrint').disable();
                    if (T1LastRec.data.CHK_STATUS == '1') {
                        if (isEditable(T1LastRec.data.CHK_YM)) {
                            Ext.getCmp('btnMedChangeUid').enable();
                            Ext.getCmp('btnWardChangeUid').enable();
                            Ext.getCmp('btnInvqty0Confirm').enable();
                        }
                    }
                    return;
                }

                if (records.length > 1) {
                    Ext.getCmp('btnPrint').disable();
                    Ext.getCmp('btnMultiPrint').enable();
                    if (isEditable(T1LastRec.data.CHK_YM)) {
                        Ext.getCmp('btnInvqty0Confirm').enable();
                    }
                    return;
                }

            },
            cellclick: function (self, td, cellIndex, record, tr, rowIndex, e, eOpts) {
                var columns = T1Grid.getColumns();
                var index = getColumnIndex(columns, 'CHK_NO');
                msglabel('');
                if (index != cellIndex) {
                    return;
                }

                // T61LastRec = record;
                T1cell = 'cell';
                //
                T1LastRec = Ext.clone(record);
                if (T1LastRec.data.CHK_WH_GRADE == '2' &&
                    T1LastRec.data.CHK_WH_KIND == '0') {
                    clearT31QueryFilter();
                    openMedDetailWindow();
                    return;
                }

                var f = T21Query.getForm();
                f.findField('T21P0').setValue(record.data.WH_NAME);
                f.findField('T21P1').setValue(record.data.CHK_YM);
                f.findField('T21P2').setValue(record.data.CHK_PERIOD_NAME);
                f.findField('T21P3').setValue(record.data.CHK_TYPE_NAME);
                f.findField('T21P4').setValue(record.data.CHK_STATUS_NAME);
                f.findField('T21P5').setValue(record.data.WH_KIND_NAME);
                f.findField('T21P6').setValue(record.data.CHK_NO);
                //f.findField('T21P7').setValue(record.data.CHK_KEEPER);

                if (Number(T1LastRec.data.CHK_STATUS) > 0 || isNaN(Number(T1LastRec.data.CHK_STATUS))) {
                    T21Grid.getColumns()[0].hide();
                    T22Grid.getColumns()[0].hide();

                    //Ext.getCmp('btnSave').disable();
                    Ext.getCmp('btnCreateSheet').disable();
                    //Ext.getCmp('btnAddToInclude').disable();
                    Ext.getCmp('btnAddAll').disable();
                    Ext.getCmp('btnPrint').enable();

                    if (T1LastRec.data.CHK_STATUS == "3" || T1LastRec.data.CHK_STATUS == "C" || T1LastRec.data.CHK_STATUS == "P") {
                        Ext.getCmp('btnAddItem').disable();
                    } else {
                        if (isEditable(T1LastRec.data.CHK_YM)) {
                            Ext.getCmp('btnAddItem').enable();
                        } else {
                            Ext.getCmp('btnAddItem').disable();
                        }

                    }

                    T21Grid.setHeight(windowHeight - 90);
                    T22Grid.setHeight(0);
                } else {
                    T21Grid.getColumns()[0].show();
                    T22Grid.getColumns()[0].show();

                    //Ext.getCmp('btnSave').enable();

                    if (isEditable(T1LastRec.data.CHK_YM)) {
                        Ext.getCmp('btnCreateSheet').enable();
                    } else {
                        Ext.getCmp('btnCreateSheet').disable();
                    }
                    if (isEditable(T1LastRec.data.CHK_YM)) {
                        Ext.getCmp('btnAddItem').enable();
                    } else {
                        Ext.getCmp('btnAddItem').disable();
                    }
                    if (isEditable(T1LastRec.data.CHK_YM)) {
                        Ext.getCmp('btnAddAll').enable();
                    } else {
                        Ext.getCmp('btnAddAll').disable();
                    }
                    Ext.getCmp('btnPrint').disable();

                    T21Grid.setHeight(windowHeight / 2 - 61);
                    T22Grid.setHeight(windowHeight / 2 - 61);
                }

                var t22form = T22Form.getForm();
                t22form.findField('F_U_PRICE').setValue(0);

                var loadT22 = (Number(T1LastRec.data.CHK_STATUS) == 0);

                T21Load(loadT22);

                if (Number(T1LastRec.data.CHK_STATUS) == 0) {
                    //T22Load();
                    T23Load();
                }

                if (T1LastRec.data.CHK_PERIOD == 'P') {
                    Ext.getCmp('btnAddFilter').show();
                    Ext.getCmp('btnAddFilter').show();
                } else {
                    Ext.getCmp('btnAddFilter').hide();
                }
                //篩選

                //if (record.data.CHK_WH_KIND == "0") {
                //    Ext.getCmp('btnDistribute').enable();
                //} else {
                //    Ext.getCmp('btnDistribute').disable();
                //}
                var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME
                detailWindow.setTitle('盤點明細管理 ' + title);

                Ext.getCmp('btnAddItem').hide();
                if (T1LastRec.data.CHK_WH_KIND == '1' && Number(T1LastRec.data.CHK_WH_GRADE) > 1 && Number(T1LastRec.data.CHK_WH_GRADE) < 3 &&
                    T1LastRec.data.CREATE_USER == 'BATCH') {
                    Ext.getCmp('btnAddItem').show();
                }

                detailWindow.show();
            },
        }
    });

    function openMedDetailWindow() {

        var record = T1LastRec;
        var f = T31Query.getForm();
        f.findField('T31P0').setValue(record.data.WH_NAME);
        f.findField('T31P1').setValue(record.data.CHK_YM);
        f.findField('T31P2').setValue(record.data.CHK_PERIOD_NAME);
        //f.findField('T31P3').setValue(record.data.CHK_TYPE_NAME);
        f.findField('T31P4').setValue(record.data.CHK_STATUS_NAME);
        //f.findField('T31P5').setValue(record.data.WH_KIND_NAME);
        f.findField('T31P6').setValue(record.data.CHK_NO);
        //f.findField('T31P7').setValue(record.data.CHK_KEEPER);

        if (Number(record.data.CHK_STATUS) > 0) {
            Ext.getCmp('btnMedCreateSheet').disable();
            Ext.getCmp('T31P8').disable();
            Ext.getCmp('predateSet').disable();
        } else {
            Ext.getCmp('btnMedCreateSheet').enable();
            Ext.getCmp('T31P8').enable();
            Ext.getCmp('predateSet').enable();
        }

        var title = record.data.CHK_NO + ' ' + record.data.WH_NAME + ' ' + record.data.CHK_YM + ' ' + record.data.CHK_PERIOD_NAME + ' ' + record.data.WH_KIND_NAME
        medDetailWindow.setTitle('盤點明細管理 ' + title);

        T31Load();

        medDetailWindow.show();
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        Ext.getCmp('chk_period_d').show();
        Ext.getCmp('chk_period_m').show();

        f.findField('CHK_WH_NO').setReadOnly(false);

        f.findField('CHK_WH_KIND_1').show();
        f.findField('CHK_WH_KIND_0').hide();
        f.findField('CHK_CLASS').hide();
        f.findField('CHK_WH_KIND_1').setValue('1');
        f.findField('CHK_PERIOD').setValue('D');

        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);

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
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '<span style="color:red">※提示</span>',
                value: '',
                renderer: function () {
                    return'<span style="color:red">藥局統一開單，無法自行新增</span>'
                }
            },
            {
                xtype: 'combo',
                store: whnoCreateStore,
                name: 'CHK_WH_NO',
                id: 'CHK_WH_NO',
                fieldLabel: '庫房代碼',
                displayField: 'WH_NAME',
                valueField: 'WH_NO',
                queryMode: 'local',
                anyMatch: true,
                allowBlank: false,
                typeAhead: true,
                forceSelection: true,
                triggerAction: 'all',
                multiSelect: false,
                fieldCls: 'required',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{WH_NAME}&nbsp;</div></tpl>',
                listeners: {
                    select: function (oldValue, newValue, eOpts) {

                        var f = T1Form.getForm();
                        f.findField('CHK_WH_GRADE').setValue(newValue.data.WH_GRADE);
                        f.findField('CHK_WH_KIND').setValue(newValue.data.WH_KIND);
                        changeChkClass('');
                        f.findField('CHK_WH_KIND_P').hide();

                        Ext.getCmp('chk_period_d').show();
                        Ext.getCmp('chk_period_m').show();
                        Ext.getCmp('chk_period_s').show();
                        Ext.getCmp('chk_period_p').show();


                        if (f.findField('CHK_WH_KIND').getValue() == '0') {     // 藥品庫


                            f.findField('CHK_WH_KIND_0').show();
                            f.findField('CHK_WH_KIND_1').hide();
                            f.findField('CHK_CLASS').hide();
                            f.findField('CHK_CLASS_E').hide();
                            f.findField('CHK_CLASS_C').hide();
                            f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '1' });

                            if (f.findField('CHK_WH_GRADE').getValue() == "2") {
                                Ext.getCmp('chk_period_d').hide();
                                Ext.getCmp('chk_period_m').hide();
                            } else {
                                Ext.getCmp('chk_period_s').hide();
                                Ext.getCmp('chk_period_p').hide();

                            }

                            Ext.getCmp('chk_wh_kind0_1').show();
                            Ext.getCmp('chk_wh_kind0_2').show();
                            Ext.getCmp('chk_wh_kind0_5').hide();
                            Ext.getCmp('chk_wh_kind0_6').hide();
                            Ext.getCmp('chk_wh_kind0_7').hide();
                            Ext.getCmp('chk_wh_kind0_8').hide();
                            if (f.findField('CHK_WH_NO').getValue() == "ANE1") {   // 麻醉科
                                //
                                Ext.getCmp('chk_wh_kind0_1').hide();
                                Ext.getCmp('chk_wh_kind0_2').hide();
                                Ext.getCmp('chk_wh_kind0_5').show();
                                Ext.getCmp('chk_wh_kind0_6').show();
                                Ext.getCmp('chk_wh_kind0_7').hide();
                                Ext.getCmp('chk_wh_kind0_8').hide();
                                f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '5' });
                            } else if (Number(f.findField('CHK_WH_GRADE').getValue()) > 2) {
                                Ext.getCmp('chk_wh_kind0_1').hide();
                                Ext.getCmp('chk_wh_kind0_2').hide();
                                Ext.getCmp('chk_wh_kind0_5').hide();
                                Ext.getCmp('chk_wh_kind0_6').hide();
                                Ext.getCmp('chk_wh_kind0_7').show();
                                Ext.getCmp('chk_wh_kind0_8').show();
                                f.findField('CHK_WH_KIND_0').setValue({ CHK_WH_KIND_0: '7' });
                            }

                        } else if (f.findField('CHK_WH_KIND').getValue() == '1') {   // 衛材庫
                            f.findField('CHK_WH_KIND_1').show();
                            f.findField('CHK_WH_KIND_0').hide();

                            Ext.getCmp('chk_period_s').hide();
                            Ext.getCmp('chk_period_p').hide();
                            Ext.getCmp('chk_period_m').hide();

                            f.findField('CHK_CLASS').hide();
                            f.findField('CHK_CLASS_E').hide();
                            f.findField('CHK_CLASS_C').hide();
                            f.findField('CHK_WH_KIND_1').setValue({ CHK_WH_KIND_1: '1' });
                            Ext.getCmp('chk_wh_kind1_0').show();
                            Ext.getCmp('chk_wh_kind1_2').show();

                            Ext.getCmp('chk_period_m').setValue(false);
                            Ext.getCmp('chk_period_d').setValue(true);
                            changeChkPeriod('D');

                            if (f.findField('CHK_WH_GRADE').getValue() == "1") {
                                f.findField('CHK_CLASS').show();
                                f.findField('CHK_CLASS').setValue({ CHK_CLASS: '02' });
                                changeChkClass('02');

                                Ext.getCmp('chk_period_m').show();
                                Ext.getCmp('chk_period_d').hide();
                                Ext.getCmp('chk_period_m').setValue(true);
                                changeChkPeriod('M');
                            }

                        } else if (f.findField('CHK_WH_KIND').getValue() == 'E') {   // 能設                              
                            f.findField('CHK_WH_KIND_1').show();
                            f.findField('CHK_WH_KIND_0').hide();

                            Ext.getCmp('chk_period_s').hide();
                            Ext.getCmp('chk_period_p').hide();
                            Ext.getCmp('chk_period_m').show();
                            Ext.getCmp('chk_period_d').hide();
                            Ext.getCmp('chk_period_m').setValue(true);
                            changeChkPeriod('M');

                            f.findField('CHK_CLASS_E').show();
                            f.findField('CHK_CLASS_C').hide();
                            f.findField('CHK_CLASS').hide();
                            f.findField('CHK_CLASS_E').setValue({ CHK_CLASS_E: '14' });
                            Ext.getCmp('chk_wh_kind1_0').hide();
                            Ext.getCmp('chk_wh_kind1_2').hide();
                            Ext.getCmp('chk_wh_kind1_1').setValue(true);
                            changeChkClass('14');

                        } else if (f.findField('CHK_WH_KIND').getValue() == 'C') {   // 通信                           
                            f.findField('CHK_WH_KIND_1').show();
                            f.findField('CHK_WH_KIND_0').hide();

                            Ext.getCmp('chk_period_s').hide();
                            Ext.getCmp('chk_period_p').hide();
                            Ext.getCmp('chk_period_m').show();
                            Ext.getCmp('chk_period_d').hide();
                            Ext.getCmp('chk_period_m').setValue(true);
                            changeChkPeriod('M');

                            f.findField('CHK_CLASS_E').hide();
                            f.findField('CHK_CLASS_C').show();
                            f.findField('CHK_CLASS').hide();
                            f.findField('CHK_CLASS_C').setValue({ CHK_CLASS_C: '30' });
                            Ext.getCmp('chk_wh_kind1_0').hide();
                            Ext.getCmp('chk_wh_kind1_2').hide();
                            Ext.getCmp('chk_wh_kind1_1').setValue(true);
                            changeChkClass('30');

                        }
                        changeChkPeriod(chkPeriod);
                    }
                }
            },
            {
                xtype: 'radiogroup',
                name: 'CHK_PERIOD',
                fieldLabel: '盤點期',
                items: [
                    { boxLabel: '日', width: 50, name: 'CHK_PERIOD', inputValue: 'D', checked: true, id: 'chk_period_d' },
                    { boxLabel: '月', width: 50, name: 'CHK_PERIOD', inputValue: 'M', id: 'chk_period_m' },
                    { boxLabel: '季', width: 50, name: 'CHK_PERIOD', inputValue: 'S', id: 'chk_period_s' },
                    { boxLabel: '抽', width: 50, name: 'CHK_PERIOD', inputValue: 'P', id: 'chk_period_p' },
                ], listeners: {
                    change: function (field, newValue, oldValue) {
                        changeChkPeriod(newValue['CHK_PERIOD']);
                    }
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '盤點年月',
                name: 'CHK_YM',
                id: 'CHK_YM',
                value: currentSetYm,
                //value: getCurrentMonth(),
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '盤點日期',
                name: 'CHK_YMD',
                id: 'CHK_YMD',
                value: getCurrentDate(),
                submitValue: true
            },
            //{
            //    xtype: 'monthfield',
            //    fieldLabel: '盤點年月',
            //    name: 'CHK_YM',
            //    id: 'CHK_YM',
            //    enforceMaxLength: true,
            //    //maxLength: 5,
            //    //minLength: 5,
            //    //regexText: '請填入民國年月',
            //    //regex: /\d{5,5}/,
            //    allowBlank:false,
            //    labelWidth: mLabelWidth,
            //    width: 180,
            //    padding: '0 4 0 4',
            //    fieldCls: 'required',
            //    //format: 'Xm',
            //    value: new Date()
            //}, 
            //{
            //    xtype: 'datefield',
            //    fieldLabel: '盤點日期',
            //    name: 'CHK_YMD',
            //    id: 'CHK_YMD',
            //    enforceMaxLength: true,
            //    //maxLength: 5,
            //    //minLength: 5,
            //    //regexText: '請填入民國年月',
            //    //regex: /\d{5,5}/,
            //    allowBlank: false,
            //    labelWidth: mLabelWidth,
            //    width: 180,
            //    padding: '0 4 0 4',
            //    fieldCls: 'required',
            //    //format: 'Xm',
            //    value: new Date(),
            //    hidden: true,
            //}, 
            {
                name: 'CHK_WH_GRADE',
                xtype: 'hidden',
                submitValue: true,
            },
            {
                name: 'CHK_WH_KIND',
                xtype: 'hidden',
                submitValue: true,
            },
            {
                xtype: 'radiogroup',
                name: 'CHK_WH_KIND_1',
                fieldLabel: '盤點類別',
                columns: 2,
                hidden: true,
                items: [
                    { boxLabel: '庫備', width: 60, name: 'CHK_WH_KIND_1', id: 'chk_wh_kind1_1', inputValue: '1', checked: true },
                    { boxLabel: '非庫備', width: 60, name: 'CHK_WH_KIND_1', id: 'chk_wh_kind1_0', inputValue: '0' },
                    { boxLabel: '小額採購', width: 80, name: 'CHK_WH_KIND_1', id: 'chk_wh_kind1_2', inputValue: '3' },
                ]
            },
            {
                xtype: 'radiogroup',
                name: 'CHK_WH_KIND_0',
                fieldLabel: '盤點類別',
                width: 150,
                columns: 2,
                items: [
                    { boxLabel: '口服', width: 60, name: 'CHK_WH_KIND_0', inputValue: '1', checked: true, id: 'chk_wh_kind0_1' },
                    { boxLabel: '非口服', width: 60, name: 'CHK_WH_KIND_0', inputValue: '2', id: 'chk_wh_kind0_2' },
                    { boxLabel: '公藥', width: 80, name: 'CHK_WH_KIND_0', inputValue: '5', hidden: true, id: 'chk_wh_kind0_5' },
                    { boxLabel: '專科', width: 80, name: 'CHK_WH_KIND_0', inputValue: '6', hidden: true, id: 'chk_wh_kind0_6' },
                    { boxLabel: '一般藥品', width: 80, name: 'CHK_WH_KIND_0', inputValue: '7', hidden: true, id: 'chk_wh_kind0_7' },
                    { boxLabel: '大瓶點滴', width: 80, name: 'CHK_WH_KIND_0', inputValue: '8', hidden: true, id: 'chk_wh_kind0_8' },
                    { boxLabel: '1~3級管制品', width: 90, name: 'CHK_WH_KIND_0', inputValue: '3', id: 'chk_wh_kind0_3' },
                    { boxLabel: '4級管制品', width: 80, name: 'CHK_WH_KIND_0', inputValue: '4', id: 'chk_wh_kind0_4' },

                ], listeners: {
                    change: function (field, newValue, oldValue) {
                        // changeChkPeriod(newValue['CHK_PERIOD']);
                    }
                }
            },
            {
                xtype: 'radiogroup',
                name: 'CHK_CLASS',
                fieldLabel: '物料分類',
                width: 150,
                columns: 2,
                hidden: true,
                items: [
                    { boxLabel: '衛材', name: 'CHK_CLASS', width: 60, inputValue: '02', checked: true },
                    { boxLabel: '被服', name: 'CHK_CLASS', width: 60, inputValue: '07' },
                    { boxLabel: '資訊耗材', name: 'CHK_CLASS', width: 80, inputValue: '08' },
                    { boxLabel: '一般物品', name: 'CHK_CLASS', width: 80, inputValue: '0X', colspan: 3 },
                ], listeners: {
                    change: function (field, newValue, oldValue) {
                        changeChkClass(newValue['CHK_CLASS']);
                    }
                }

            },
            {
                xtype: 'radiogroup',
                name: 'CHK_CLASS_E',
                fieldLabel: '物料分類',
                width: 150,
                columns: 2,
                hidden: true,
                items: [
                    { boxLabel: '水電', name: 'CHK_CLASS_E', width: 60, inputValue: '14' },
                    { boxLabel: '空調', name: 'CHK_CLASS_E', width: 60, inputValue: '15' },
                    { boxLabel: '中控', name: 'CHK_CLASS_E', width: 60, inputValue: '16' },
                    { boxLabel: '工務', name: 'CHK_CLASS_E', width: 60, inputValue: '17' },
                    { boxLabel: '污被服', name: 'CHK_CLASS_E', width: 70, inputValue: '18' },
                    { boxLabel: '鍋爐', name: 'CHK_CLASS_E', width: 60, inputValue: '19' },
                    { boxLabel: '氣體', name: 'CHK_CLASS_E', width: 60, inputValue: '20' },
                    { boxLabel: '電梯', name: 'CHK_CLASS_E', width: 60, inputValue: '21' },
                    { boxLabel: '公共天線', name: 'CHK_CLASS_E', width: 80, inputValue: '22' },
                    { boxLabel: '廣播', name: 'CHK_CLASS_E', width: 60, inputValue: '23' },
                ], listeners: {
                    change: function (field, newValue, oldValue) {
                        changeChkClass(newValue['CHK_CLASS_E']);
                    }
                }

            },
            {
                xtype: 'radiogroup',
                name: 'CHK_CLASS_C',
                fieldLabel: '物料分類',
                width: 150,
                columns: 2,
                hidden: true,
                items: [
                    { boxLabel: '對講機', name: 'CHK_CLASS_C', width: 70, inputValue: '30' },
                    { boxLabel: '子母鐘', name: 'CHK_CLASS_C', width: 70, inputValue: '31' },
                    { boxLabel: '叫號燈', name: 'CHK_CLASS_C', width: 70, inputValue: '32' },
                    { boxLabel: '護士呼叫', name: 'CHK_CLASS_C', width: 80, inputValue: '33' },
                    { boxLabel: '電話交換機', name: 'CHK_CLASS_C', width: 90, inputValue: '34' },
                    { boxLabel: '呼叫器', name: 'CHK_CLASS_C', width: 70, inputValue: '35' },
                    { boxLabel: '膠紙', name: 'CHK_CLASS_C', width: 60, inputValue: '36' },
                    { boxLabel: '膠管', name: 'CHK_CLASS_C', width: 60, inputValue: '37' },
                ], listeners: {
                    change: function (field, newValue, oldValue) {
                        changeChkClass(newValue['CHK_CLASS_C']);
                    }
                }

            },
            {
                xtype: 'displayfield',
                name: 'CHK_WH_KIND_P',
                fieldLabel: '盤點類別',
                hidden: true,
                width: 150,
                value: '不分類'
            },
            {
                xtype: 'hidden',
                name: 'CHK_TYPE',
                submitValue: true
            },

            {
                fieldLabel: '負責人員',
                xtype: 'displayfield',
                name: 'CHK_KEEPER',
                value: userName
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    var f = T1Form.getForm();
                    if (!f.findField('CHK_WH_NO').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">庫房代碼</span>為必填');
                        return;
                    }

                    if (!f.findField('CHK_PERIOD').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點期</span>為必填');
                        return;
                    }

                    if (chkPeriod == 'D' &&
                        !f.findField('CHK_YMD').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點日期</span>為必填');
                        return;
                    }

                    if (chkPeriod != 'D' &&
                        !f.findField('CHK_YM').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點年月</span>為必填');
                        return;
                    }

                    if (f.findField('CHK_WH_KIND').getValue() == '1' &&
                        !f.findField('CHK_WH_KIND_1').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點類別</span>為必填');
                        return;
                    }

                    if (f.findField('CHK_WH_KIND').getValue() == '0' &&
                        !f.findField('CHK_WH_KIND_0').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點類別</span>為必填');
                        return;
                    }
                    //
                    if (f.findField('CHK_WH_KIND').getValue() == '0' &&
                        f.findField('CHK_WH_GRADE').getValue() == '2' &&
                        chkPeriod != 'P' &&
                        chkPeriod != 'S') {
                        Ext.Msg.alert('提醒', '<span style="color:red">盤點期</span>為必填');
                        return;
                    }

                    if (f.findField('CHK_WH_KIND').getValue() == '1' &&
                        f.findField('CHK_WH_GRADE').getValue() == '1' &&
                        !f.findField('CHK_CLASS').getValue()) {
                        Ext.Msg.alert('提醒', '<span style="color:red">物料分類</span>為必填');
                        return;
                    }

                    if (f.findField('CHK_WH_KIND').getValue() == '1' &&
                        f.findField('CHK_WH_GRADE').getValue() == '2') {
                        chkClass = '02';
                    }


                    if (this.up('form').getForm().isValid()) {

                        if (f.findField('CHK_WH_GRADE').getValue() == '1'
                            && f.findField('CHK_WH_KIND').getValue() != 'E'
                            && f.findField('CHK_WH_KIND').getValue() != 'C') {

                            if (f.findField('CHK_WH_KIND').getValue() == '0') {
                                var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                                Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                    if (btn === 'yes') {
                                        T1Submit();
                                    }
                                });
                                return;
                            }

                            // 中央庫房檢查是否有在途量
                            Ext.Ajax.request({
                                url: '/api/CE0002/CheckMeDocm5',
                                method: reqVal_p,
                                params: {
                                    wh_no: f.findField('CHK_WH_NO').getValue(),
                                    wh_kind: f.findField('CHK_WH_KIND').getValue(),
                                    chk_type: f.findField('CHK_WH_KIND_1').getValue(), //    物料分類
                                    chk_class: f.findField('CHK_CLASS').getValue(), //    盤點類別: 庫備、非庫備、小額採購
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        if (data.afrs == 0) {
                                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                                if (btn === 'yes') {
                                                    T1Submit();
                                                }
                                            });
                                            return;
                                            return;
                                        }

                                        Ext.MessageBox.confirm('提示', '尚有未確認之申請單<br/>是否將在途量自動分歸？', function (btn, text) {
                                            if (btn === 'yes') {
                                                //transferToBcwhpick("");
                                                copeOnWayQty(T1Query.getForm().findField('P0').getValue(),
                                                    f.findField('CHK_WH_KIND_1').getValue(),
                                                    f.findField('CHK_CLASS').getValue());
                                            }
                                        });
                                    } else {
                                        Ext.Msg.alert('失敗', '發生例外錯誤');
                                    }

                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('失敗', '發生例外錯誤');
                                }
                            });
                            return;
                        }

                        if (f.findField('CHK_WH_GRADE').getValue() == '2' && f.findField('CHK_WH_KIND').getValue() == '0') {
                            var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                            Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                if (btn === 'yes') {
                                    T1Submit();
                                }
                            });
                            return;
                        }

                        // 2020-12-09: 科室病房開單前檢查在途量，有的話不可開單
                        Ext.Ajax.request({
                            url: '/api/CE0002/CheckOnwayQty',
                            method: reqVal_p,
                            params: {
                                wh_no: f.findField('CHK_WH_NO').getValue(),
                            },
                            success: function (response) {
                                
                                var data = Ext.decode(response.responseText);
                                if (data.success) {

                                    if (data.afrs > 0) {
                                        var whItem = whnoQueryStore.findRecord('WH_NO', f.findField('CHK_WH_NO').getValue());
                                        Ext.Msg.alert('提示', (whItem.data.WH_NAME + '<span style="color:red">尚有未點收品項</span>，請先點收後再開立盤點單'));
                                        return;
                                    }

                                    temp_mast = {
                                        CHK_WH_NO: f.findField('CHK_WH_NO').getValue(),
                                        CHK_YM: f.findField('CHK_YM').rawValue,
                                        CHK_YMD: f.findField('CHK_YMD').rawValue,
                                        CHK_WH_GRADE: f.findField('CHK_WH_GRADE').getValue(),
                                        CHK_WH_KIND: f.findField('CHK_WH_KIND').getValue(),
                                        CHK_PERIOD: chkPeriod,
                                        CHK_CLASS: f.findField('CHK_WH_KIND').getValue() == '0' ? '01' : chkClass,
                                        CHK_LEVEL: '1'
                                    };

                                    if (f.findField('CHK_WH_KIND').getValue() == 'C' || f.findField('CHK_WH_KIND').getValue() == 'E') {
                                        temp_mast.CHK_TYPE = f.findField('CHK_WH_KIND_1').getValue()['CHK_WH_KIND_1'];
                                    } else if (f.findField('CHK_WH_KIND').getValue() == '1') {
                                        temp_mast.CHK_TYPE = f.findField('CHK_WH_KIND_1').getValue()['CHK_WH_KIND_1'];
                                    } else {
                                        temp_mast.CHK_TYPE = f.findField('CHK_WH_KIND_0').getValue()['CHK_WH_KIND_0'];
                                        if (temp_mast.CHK_WH_GRADE == '2' && (temp_mast.CHK_PERIOD == 'P' || temp_mast.CHK_PERIOD == 'S')) {
                                            temp_mast.CHK_TYPE = 'X';
                                        }
                                    }


                                    if (f.findField('CHK_WH_KIND').getValue() == 'E'
                                        || f.findField('CHK_WH_KIND').getValue() == 'C') {
                                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                                            if (btn === 'yes') {
                                                oneClickCreateSheet([]);
                                            }
                                        });
                                        return;
                                    }

                                    T23LoadOneClick(f.findField('CHK_WH_NO').getValue());
                                    oneClickPickUserWindow.show();

                                    
                                } else {
                                    Ext.Msg.alert('失敗', '發生例外錯誤');
                                }

                            },
                            failure: function (response, options) {
                                Ext.Msg.alert('失敗', '發生例外錯誤');
                            }
                        });
                        
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });
    function changeChkPeriod(period) {
        chkPeriod = period;
        Ext.getCmp('CHK_YM').setValue(currentSetYm);
        if (period == "D") {
            Ext.getCmp('CHK_YMD').show();
            Ext.getCmp('CHK_YM').hide();
        } else {
            Ext.getCmp('CHK_YMD').hide();
            Ext.getCmp('CHK_YM').show();
        }
        //
        if (T1Form.getForm().findField('CHK_WH_KIND').getValue() == '0' &&
            T1Form.getForm().findField('CHK_WH_GRADE').getValue() == '2' &&
            (chkPeriod == 'P' || chkPeriod == 'S')) {
            T1Form.getForm().findField('CHK_WH_KIND_P').show();
            T1Form.getForm().findField('CHK_WH_KIND_0').hide();
        } else if (T1Form.getForm().findField('CHK_WH_KIND').getValue() == '0') {
            T1Form.getForm().findField('CHK_WH_KIND_P').hide();
            T1Form.getForm().findField('CHK_WH_KIND_0').show();
        }

        
        if (T1Form.getForm().findField('CHK_WH_KIND').getValue() == '0'){
            Ext.getCmp('chk_wh_kind0_3').show();
        } 
        if (T1Form.getForm().findField('CHK_WH_KIND').getValue() == '0' &&
            T1Form.getForm().findField('CHK_WH_GRADE').getValue() == '2') {
            Ext.getCmp('chk_wh_kind0_3').hide();
        }
        
        if (T1Form.getForm().findField('CHK_WH_KIND').getValue() == '0' &&
            (T1Form.getForm().findField('CHK_WH_GRADE').getValue() == '3' || T1Form.getForm().findField('CHK_WH_GRADE').getValue() == '4') &&
            period == 'M') {
            Ext.getCmp('chk_wh_kind0_3').hide();
        }
    }
    function changeChkClass(inputClass) {
        chkClass = inputClass;
    }
    function changeOrderWay(orderwayInput) {
        orderway = orderwayInput;
    }
    function changeIsDistri(isDistriInput) {
        is_distri = isDistriInput;
        Ext.getCmp('T23Grid').hide();
        if (is_distri == 'true') {
            Ext.getCmp('T23Grid').show();
        }
    }

    function T1Submit() {

        var f = T1Form.getForm();
        //var chkClasss = (f.findField('CHK_WH_GRADE').getValue() == '1' &&
        //    f.findField('CHK_WH_KIND').getValue() == '1') ?
        //    f.findField('CHK_CLASS').getValue() : '';
        var data = {
            CHK_WH_NO: f.findField('CHK_WH_NO').getValue(),
            CHK_YM: f.findField('CHK_YM').rawValue,
            CHK_YMD: f.findField('CHK_YMD').rawValue,
            CHK_WH_GRADE: f.findField('CHK_WH_GRADE').getValue(),
            CHK_WH_KIND: f.findField('CHK_WH_KIND').getValue(),
            CHK_PERIOD: chkPeriod,
            CHK_CLASS: f.findField('CHK_WH_KIND').getValue() == '0' ? '01' : chkClass,
            CHK_LEVEL: '1'
        };

        if (f.findField('CHK_WH_KIND').getValue() == 'C' || f.findField('CHK_WH_KIND').getValue() == 'E') {
            data.CHK_TYPE = f.findField('CHK_WH_KIND_1').getValue()['CHK_WH_KIND_1'];
        } else if (f.findField('CHK_WH_KIND').getValue() == '1') {
            data.CHK_TYPE = f.findField('CHK_WH_KIND_1').getValue()['CHK_WH_KIND_1'];
        } else {
            data.CHK_TYPE = f.findField('CHK_WH_KIND_0').getValue()['CHK_WH_KIND_0'];
            if (data.CHK_WH_GRADE == '2' && (data.CHK_PERIOD == 'P' || data.CHK_PERIOD == 'S')) {
                data.CHK_TYPE = 'X';
            }
        }
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/InsertMaster',
            method: reqVal_p,
            params: data,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    myMask.hide();
                    Ext.Msg.alert('失敗', data.msg);
                    return;
                }


                myMask.hide();

                T1Query.getForm().findField('P0').setValue(f.findField('CHK_WH_NO').getValue());
                T1Query.getForm().findField('P1').setValue(f.findField('CHK_YM').rawValue);

                T1Load(false);
                msglabel('資料新增成功');

                T1Cleanup();
            },
            failure: function (response, options) {
                myMask.hide();
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    function getCurrentDate() {
        var today = new Date();
        var yyyy = today.getFullYear();
        var m = today.getMonth() + 1;
        var d = today.getDate();
        var mm = m > 9 ? m.toString() : "0" + m;
        var dd = d > 9 ? d.toString() : "0" + d;
        return (yyyy - 1911).toString() + mm + dd;
    }
    function getCurrentMonth() {
        Ext.Ajax.request({
            url: '/api/CE0002/GetCurrentSetYm',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                //Ext.getCmp('CHK_YM').setValue(data.msg);
                currentSetYm = data.msg
            },
            failure: function (response, options) {
                myMask.hide();
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
        //var today = new Date();
        //var yyyy = today.getFullYear();
        //var m = today.getMonth() + 1;
        //var mm = m > 9 ? m.toString() : "0" + m;
        //return (yyyy - 1911).toString() + mm;

    }
    getCurrentMonth();

    function isEditable(chk_ym) {

        if (chk_ym.substring(0, 5) != todayDateString) {
            return false;
        } else {
            return true;
        }
    }

    // #region 盤點項目編輯
    var T21Store = viewModel.getStore('DetailInclude');
    T21Store.on('beforeload', function (store, options) {

        T21Store.getProxy().setExtraParam('windowNewOpen', windowNewOpen == true ? 'Y' : 'N');

        windowNewOpen = false;
    });
    function T21Load(isLoadT22) {
        loadT22 = isLoadT22;
        T21Store.removeAll();
        T22Store.removeAll();

        //T21Query.getForm().findField('T1P0').setValue(T1Query.getForm().findField('P0').getValue());
        T21Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T21Store.getProxy().setExtraParam("chk_status", T1LastRec.data.CHK_STATUS);

        T21Tool.moveFirst();
    }
    T21Store.on('load', function (store, options) {
        if (loadT22) {
            T22Load(true);
        }
    });
    var T22Store = viewModel.getStore('DetailExclude');
    function T22Load(isMoveFirst) {
        var f = T22Form.getForm();
        T22Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);
        T22Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T22Store.getProxy().setExtraParam("chk_wh_grade", T1LastRec.data.CHK_WH_GRADE);
        T22Store.getProxy().setExtraParam("chk_wh_kind", T1LastRec.data.CHK_WH_KIND);
        T22Store.getProxy().setExtraParam("chk_type", T1LastRec.data.CHK_TYPE);
        T22Store.getProxy().setExtraParam("chk_class", T1LastRec.data.CHK_CLASS);
        T22Store.getProxy().setExtraParam("chk_period", T1LastRec.data.CHK_PERIOD);
        T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());

        if (isMoveFirst) {
            T22Tool.moveFirst();
        } else {
            T22Store.load({
                params: {
                    start: 0
                }
            });
        }
    }
    var T23Store = viewModel.getStore('PickUsers');
    function T23Load() {

        T23Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);

        T23Tool.moveFirst();
    }
    function T23LoadOneClick(wh_no) {

        T23Store.getProxy().setExtraParam("wh_no", wh_no);

        T23Tool.moveFirst();
    }
    var T21Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP21',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            name: 'T21P0',
                            id: 'T21P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T21P1',
                            id: 'T21P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點期',
                            name: 'T21P2',
                            id: 'T21P2',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點類別',
                            name: 'T21P3',
                            id: 'T21P3',
                            labelWidth: 60,
                            width: 150,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            name: 'T21P4',
                            id: 'T21P4',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP22',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            name: 'T21P6',
                            id: 'T21P6',
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'T21P5',
                            id: 'T21P5',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            fieldLabel: '庫房分類',
                            labelAlign: 'right',
                        },
                    ]
                }
            ]
        }]
    });
    var T22Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP23',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            name: 'ORI_F_U_PRICE',
                            xtype: 'hidden',
                            value: 0
                        },

                        {
                            xtype: 'numberfield',
                            fieldLabel: '單價 大於',
                            name: 'F_U_PRICE',
                            enforceMaxLength: false,
                            maxLength: 14,
                            hideTrigger: true,
                            value: 0
                        }
                    ]
                },
            ]
        }]
    });
    var T23Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP24',
                    border: false,
                    layout: 'vbox',
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'IS_DISTRI',
                            fieldLabel: '是否分配人員',
                            items: [
                                { boxLabel: '否', width: 50, name: 'IS_DISTRI', inputValue: 'false', checked: true },
                                { boxLabel: '是', width: 50, name: 'IS_DISTRI', inputValue: 'true' }
                            ], listeners: {
                                change: function (field, newValue, oldValue) {

                                    changeIsDistri(newValue['IS_DISTRI']);
                                    Ext.getCmp('ORDERWAY_radio').hide();
                                    if (newValue['IS_DISTRI'] == 'true') {
                                        orderway = 'STORE_LOC';
                                        Ext.getCmp('ORDERWAY_radio').show();
                                    }
                                }
                            }
                        },
                        {
                            xtype: 'radiogroup',
                            id: 'ORDERWAY_radio',
                            name: 'ORDERWAY',
                            fieldLabel: '排序方式',
                            hidden: true,
                            items: [
                                { boxLabel: '儲位', width: 50, name: 'ORDERWAY', inputValue: 'STORE_LOC', checked: true },
                                { boxLabel: '院內碼', width: 80, name: 'ORDERWAY', inputValue: 'MMCODE' },
                                { boxLabel: '電腦量', width: 80, name: 'ORDERWAY', inputValue: 'INV_QTY' },

                            ], listeners: {
                                change: function (field, newValue, oldValue) {
                                    changeOrderWay(newValue['ORDERWAY']);
                                }
                            }
                        }
                    ]
                },
            ]
        }]
    });
    // 印表排序清單
    var printStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT'],
        data: [
            { "VALUE": "mmcode, store_loc, chk_uid", "TEXT": "院內碼 + 儲位 + 盤點人員" },
            { "VALUE": "mmcode, chk_uid, store_loc", "TEXT": "院內碼 + 盤點人員 + 儲位" },
            { "VALUE": "store_loc, mmcode, chk_uid", "TEXT": "儲位 + 院內碼 + 盤點人員" },
            { "VALUE": "store_loc, chk_uid, mmcode", "TEXT": "儲位 + 盤點人員 + 院內碼" },
            { "VALUE": "chk_uid, mmcode, store_loc", "TEXT": "盤點人員 + 院內碼 + 儲位" },
            { "VALUE": "chk_uid, store_loc, mmcode", "TEXT": "盤點人員 + 儲位 + 院內碼" },
        ]
    });
    var T24Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP25',
                    border: false,
                    layout: 'hbox',
                    width: 250,
                    items: [
                        {
                            xtype: 'combo',
                            store: printStore,
                            name: 'print_order',
                            id: 'print_order',
                            fieldLabel: '印表排序',
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            allowBlank: false,
                            typeAhead: true,
                            forceSelection: true,
                            triggerAction: 'all',
                            multiSelect: false,
                            width: '100%',
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                        },
                    ]
                },
            ]
        }]
    });
    var T21Tool = Ext.create('Ext.PagingToolbar', {
        store: T21Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '產生盤點單',
                id: 'btnCreateSheet',
                name: 'btnCreateSheet',
                handler: function () {
                    if (T21Grid.getStore().data.items.length == 0) {
                        Ext.Msg.alert('提醒', '已選定盤點清單為空，請先選擇項目加入清單');
                        return;
                    }

                    if (T22Grid.getStore().data.items.length > 0) {
                        Ext.Msg.alert('提醒', '尚有未盤點項目，請再次確認');
                        return;
                    }

                    if (T1LastRec.data.CHK_WH_KIND == "0") {

                        T23Grid.getSelectionModel().deselectAll();
                        T23Form.getForm().findField('IS_DISTRI').setValue({ IS_DISTRI: 'false' });
                        T23Form.getForm().findField('ORDERWAY').setValue({ ORDERWAY: 'STORE_LOC' });
                        changeOrderWay('STORE_LOC');
                        changeIsDistri('false');
                        pickUserWindow.show();
                        return;
                    }

                    if (T1LastRec.data.CHK_WH_KIND == "1" && T1LastRec.data.CHK_WH_GRADE != '1') {

                        T23Grid.getSelectionModel().deselectAll();

                        T23Form.getForm().findField('ORDERWAY').setValue('MMCODE');
                        changeOrderWay('MMCODE');
                        pickUserWindow.show();
                        return;
                    }

                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            createSheet(T21Grid.getStore().data.items, [], T1LastRec.data.CHK_WH_KIND);
                        }
                    }
                    );
                }
            },
            {
                text: '加入項目',
                id: 'btnAddItem',
                name: 'btnAddItem',
                hidden: true,
                handler: function () {

                    T41Form.getForm().findField('MMCODE').setValue('');
                    T41Load();
                    addItemWindow.show();
                }
            }

        ]
    });
    var T22Tool = Ext.create('Ext.PagingToolbar', {
        store: T22Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '篩選設定',
                id: 'btnAddFilter',
                name: 'btnAddFilter',
                handler: function () {
                    filterWindow.show();
                }
            },
            {
                text: '全盤',
                id: 'btnAddAll',
                name: 'btnAddAll',
                handler: function () {
                    Ext.MessageBox.confirm('全盤', '確定全部加入盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            addAll();
                        }
                    }
                    );
                }
            },
        ]
    });
    var T23Tool = Ext.create('Ext.PagingToolbar', {
        store: T23Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T21Grid = Ext.create('Ext.grid.Panel', {
        store: T21Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 65,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T21Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "開單時電腦量",
                dataIndex: 'INV_QTY',
                //width: 60,
                align: 'right',
                style: 'text-align:left',
                width: 80,
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC_NAME',
                width: 120,
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 150,
            }
        ],
    });
    var T22Grid = Ext.create('Ext.grid.Panel', {
        store: T22Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: windowHeight / 2 - 62,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T22Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "電腦量",
                dataIndex: 'INV_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "盤點量",
                dataIndex: 'CHK_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "儲位",
                dataIndex: 'STORE_LOC',
                xtype: 'templatecolumn',
                tpl: '{STORE_LOC_NAME}',
                width: 120,
            },
            {
                text: "盤點人員",
                dataIndex: 'CHK_UID_NAME',
                width: 150,
            }
        ],
    });
    var T23Grid = Ext.create('Ext.grid.Panel', {
        store: T23Store,
        id: 'T23Grid',
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 380,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T23Tool]
            }
        ],
        columns: [
            //{
            //    text: "員工代碼",
            //    dataIndex: 'WH_CHKUID',
            //    width: 120
            //},
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    var detailSave = function () {
        //var list = [];
        //for (var i = 0; i < gridStore.length; i++) {
        //    var item = gridStore[i].data;
        //    item.CHK_NO = T1LastRec.data.CHK_NO;
        //    list.push(item);
        //}

        //if (list.length == 0) {
        //    list.push({ CHK_NO: T1LastRec.data.CHK_NO });
        //}

        Ext.Ajax.request({
            url: '/api/CE0002/DetailSave',
            method: reqVal_p,
            params: {
                chk_no: T1LastRec.data.CHK_NO
            },
            success: function (response) {
                windowNewOpen = true;
                T21Load(true);
                //T22Load();

                msglabel("暫存明細成功");
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });

    }

    var createSheet = function (detailStore, userStore, wh_kind) {
        var list = [];
        for (var i = 0; i < detailStore.length; i++) {
            var item = detailStore[i].data;
            item.CHK_NO = T1LastRec.data.CHK_NO;
            list.push(item);
        }

        var users = [];
        if (T1LastRec.data.CHK_WH_NO != '560000') {
            for (var i = 0; i < userStore.length; i++) {
                users.push(userStore[i].data);
            }
        }
        var myMask = new Ext.LoadMask(Ext.getCmp('pickUserWindow'), { msg: '處理中...' });
        myMask.show();
        var myMask1 = new Ext.LoadMask(Ext.getCmp('detailWindow'), { msg: '處理中...' });
        myMask1.show();

        Ext.Ajax.request({
            url: '/api/CE0002/CreateSheet',
            method: reqVal_p,
            params: {
                details: Ext.util.JSON.encode(list),
                users: Ext.util.JSON.encode(users),
                chk_wh_kind: wh_kind,
                chk_wh_grade: T1LastRec.data.CHK_WH_GRADE,
                orderway: orderway,
                chk_no: T1LastRec.data.CHK_NO,
                is_distri: is_distri
            },
            success: function (response) {
                myMask.hide();
                myMask1.hide();
                windowNewOpen = true;
                T1LastRec.data.CHK_STATUS = '1';
                T21Load(true);
                //T22Load();
                T1Load(false);

                pickUserWindow.hide();
                detailWindow.hide();

                msglabel("產生盤點單成功");

                T21Grid.getColumns()[0].hide();
                T22Grid.getColumns()[0].hide();
                //Ext.getCmp('btnSave').disable();
                Ext.getCmp('btnCreateSheet').disable();
                //Ext.getCmp('btnAddToInclude').disable();
                Ext.getCmp('btnAddAll').disable();

                Ext.Ajax.request({
                    url: '/api/CE0002/DeleteDetailTemps',
                    method: reqVal_p,
                    params: {
                        chk_no: T1LastRec.data.CHK_NO
                    },
                    success: function (response) {
                        //T21Load(true);
                        //T22Load();
                        detailWindow.hide();
                        windowNewOpen = true;
                        //T1Load();

                        //msglabel("暫存明細成功");
                    },
                    failure: function (response, options) {
                        //Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });

            },
            failure: function (response, options) {
                myMask.hide();
                // Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var addAll = function () {
        //
        Ext.Ajax.request({
            url: '/api/CE0002/AddAll',
            method: reqVal_p,
            params: {
                wh_no: T1LastRec.data.CHK_WH_NO,
                chk_no: T1LastRec.data.CHK_NO,
                chk_wh_grade: T1LastRec.data.CHK_WH_GRADE,
                chk_wh_kind: T1LastRec.data.CHK_WH_KIND,
                chk_type: T1LastRec.data.CHK_TYPE,
                chk_class: T1LastRec.data.CHK_CLASS,
            },
            success: function (response) {

                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    Ext.Msg.alert('失敗', data.msg);
                    return;
                }
                windowNewOpen = true;
                T21Load(true);
                //T22Load();

                msglabel("全部加入盤點單成功");
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }

    var detailWindow = Ext.create('Ext.window.Window', {
        id: 'detailWindow',
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't21Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '已選定盤點清單',
                        border: false,
                        height: '49%',
                        minHeight: windowHeight / 2 - 90,
                        items: [T21Grid]
                    },
                    {
                        xtype: 'splitter',
                        collapseTarget: 'dev'
                    },
                    {
                        xtype: 'form',
                        autoScroll: true,
                        itemId: 't22Grid',
                        region: 'south',
                        layout: 'fit',
                        //collapsible: true,
                        title: '未選定盤點清單',
                        //titleCollapse: true,
                        border: false,
                        height: '50%',
                        minHeight: 30,
                        split: true,
                        items: [T22Grid]
                    }
                ],
            }
        ],

        //items: [T21Grid, T22Grid],
        width: "900px",
        height: windowHeight,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                //
                Ext.Ajax.request({
                    url: '/api/CE0002/DeleteDetailTemps',
                    method: reqVal_p,
                    params: {
                        chk_no: T1LastRec.data.CHK_NO
                    },
                    success: function (response) {
                        //T21Load(true);
                        //T22Load();
                        detailWindow.hide();
                        windowNewOpen = true;
                        T1Load();

                        //msglabel("暫存明細成功");
                    },
                    failure: function (response, options) {
                        Ext.Msg.alert('失敗', '發生例外錯誤');
                    }
                });


            }
        }],
        listeners: {
            show: function (self, eOpts) {
                detailWindow.setY(0);
            }
        }
    });
    detailWindow.hide();

    var pickUserWindow = Ext.create('Ext.window.Window', {
        id: 'pickUserWindow',
        renderTo: Ext.getBody(),
        items: [T23Form,
            T23Grid
        ],
        modal: true,
        width: "400px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        title: "盤點人員挑選",
        buttons: [
            {
                text: '確定',
                id: 'btnSetPickUser',
                name: 'btnSetPickUser',
                handler: function () {
                    var selection = T23Grid.getSelection();

                    if (is_distri == 'true') {
                        if (selection.length == 0) {
                            Ext.Msg.alert('提醒', '請選擇盤點人員');
                            return;
                        }
                    }

                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            createSheet(T21Grid.getStore().data.items, selection, T1LastRec.data.CHK_WH_KIND);
                        }
                    }
                    );
                }
            }
            , {
                text: '關閉',
                handler: function () {
                    pickUserWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                pickUserWindow.center();
                pickUserWindow.setWidth(400);
            }
        }
    });
    pickUserWindow.hide();

    var filterWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T22Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "篩選條件設定",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var f = T22Form.getForm();
                    if (f.findField('F_U_PRICE').getValue() == null) {
                        f.findField('F_U_PRICE').setValue(0);
                    }

                    f.findField('ORI_F_U_PRICE').setValue(f.findField('F_U_PRICE').getValue());

                    T22Store.getProxy().setExtraParam("F_U_PRICE", f.findField('F_U_PRICE').getValue());

                    T22Load(true);

                    filterWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    var f = T22Form.getForm();
                    if (f.findField('F_U_PRICE').getValue() == null) {
                        f.findField('F_U_PRICE').setValue(0);
                    }

                    f.findField('F_U_PRICE').setValue(f.findField('ORI_F_U_PRICE').getValue());

                    filterWindow.hide();
                }
            }
        ]
    });
    filterWindow.hide();

    var printWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [T24Form],
        resizable: false,
        draggable: false,
        closable: false,
        title: "列印設定",
        buttons: [
            {
                text: '確定',
                handler: function () {
                    if (!T24Form.getForm().findField('print_order').getValue()) {
                        Ext.Msg.alert('提醒', '請選擇印表排序方式');
                        return;
                    }

                    if (T1LastRec.data.CHK_WH_NO == 'PH1S') {
                        showReport(reportUrlPH1S, T1LastRec.data.CHK_NO, "");
                    } else if (T1LastRec.data.CHK_WH_GRADE == '1') {
                        showReport(reportUrl, T1LastRec.data.CHK_NO, "");
                    } else {
                        showReport(reportUrlWard, T1LastRec.data.CHK_NO, "");
                    }
                    printWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    printWindow.hide();
                }
            }
        ]
    });
    printWindow.hide();

    function showReport(url, chk_no, chk_nos) {
        if (!win) {

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + url + '?CHK_NO=' + chk_no
                + '&PRINT_ORDER=' + T24Form.getForm().findField('print_order').getValue()
                + '&CHK_NOS=' + chk_nos
                + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    // #endregion --------------------------

    // #region 藥局藥品庫
    var T31QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'T31P11',
        name: 'T31P11',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        width: 160,
        labelWidth:60,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T31Query.getForm().findField('T31P6').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });

    var T31QuryMMCode2 = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'T31P12',
        name: 'T31P12',
        fieldLabel: '至',
        labelAlign: 'right',
        labelWidth: 7,
        width: 107,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/CE0002/GetMMCODECombo',//指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T31Query.getForm().findField('T31P6').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T31Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP31',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點單號',
                            name: 'T31P6',
                            id: 'T31P6',
                            labelWidth: 60,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            name: 'T31P0',
                            id: 'T31P0',
                            labelWidth: 60,
                            width: 180,
                            fieldLabel: '庫房代碼',
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點年月',
                            name: 'T31P1',
                            id: 'T31P1',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '盤點期',
                            name: 'T31P2',
                            id: 'T31P2',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        },
                        {
                            xtype: 'displayfield',
                            fieldLabel: '狀態',
                            name: 'T31P4',
                            id: 'T31P4',
                            labelWidth: 60,
                            width: 130,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                        }
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP22',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'numberfield',
                            fieldLabel: '項次',
                            name: 'T31P9',
                            id: 'T31P9',
                            labelWidth: 60,
                            width: 110,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: 1,
                            hideTrigger: true
                        },
                        {
                            xtype: 'numberfield',
                            fieldLabel: '至',
                            name: 'T31P10',
                            id: 'T31P10',
                            labelSeparator :'',
                            labelWidth: 7,
                            width: 57,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: 1,
                            hideTrigger: true
                        },
                        T31QuryMMCode,
                        T31QuryMMCode2,
                        {
                            xtype: 'container',
                            defaultType: 'checkboxfield',
                            layout: 'hbox',
                            items: [
                                {
                                    boxLabel: '僅顯示未設定日期品項',
                                    name: 'chkpredateNullOnly',
                                    inputValue: 'Y',
                                    id: 'chkpredateNullOnly',
                                    width: 140,
                                    padding: '0 0 0 20'
                                }
                            ]
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            id: 'btnMedDetailQueryLoad',
                            handler: function () {
                                T31Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            id: 'btnMedDetailQueryClear',
                            handler: function () {
                                clearT31QueryFilter();
                            }
                        },
                        //{
                        //    xtype: 'numberfield',
                        //    fieldLabel: '至',
                        //    name: 'T31P9',
                        //    id: 'T31P9',
                        //    labelWidth: 10,
                        //    width: 130,
                        //    padding: '0 4 0 4',
                        //    labelAlign: 'right',
                        //},
                        
                    ]
                },
                {
                    xtype: 'panel',
                    id: 'PanelP32',
                    border: false,
                    layout: 'hbox',
                    hidden: false,
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '預計盤點日期',
                            name: 'T31P8',
                            id: 'T31P8',
                            labelWidth: 80,
                            width: 180,
                            padding: '0 4 0 4',
                            labelAlign: 'right',
                            minValue: new Date()
                        },
                        {
                            xtype: 'button',
                            text: '設定',
                            id: 'predateSet',
                            handler: function () {
                                //
                                if (T31Query.getForm().findField('T31P8').getValue() == null || T31Query.getForm().findField('T31P8').getValue() == undefined) {
                                    Ext.Msg.alert('提醒', '請選擇預計盤點日期');
                                    return;
                                }
                                
                                if (T31Query.getForm().findField('T31P8').isValid() == false) {
                                    Ext.Msg.alert('提醒', '日期小於今天或格式錯誤，請重新選擇');
                                    return;
                                }

                                var selections = T31Grid.getSelection();

                                if (selections.length == 0) {
                                    Ext.Msg.alert('提醒', '請選擇欲設定日期之項目');
                                    return;
                                }
                                var chk_pre_date = getDateFormat(T31Query.getForm().findField('T31P8').getValue());

                                var data = [];
                                for (var i = 0; i < selections.length; i++) {
                                    var item = selections[i].data;
                                    item.CHK_PRE_DATE = chk_pre_date;
                                    data.push(selections[i].data);
                                }

                                var myMask = new Ext.LoadMask(Ext.getCmp('medDetailWindow'), { msg: '處理中...' });
                                myMask.show();

                                Ext.Ajax.request({
                                    url: '/api/CE0002/SetPreDate',
                                    method: reqVal_p,
                                    params: {
                                        item_string: Ext.util.JSON.encode(data)
                                    },
                                    success: function (response) {
                                        myMask.hide();
                                        T31Load();
                                        msglabel("設定預計盤點時間成功");
                                    },
                                    failure: function (response, options) {
                                        Ext.Msg.alert('失敗', '發生例外錯誤');
                                    }
                                });
                            }
                        },
                    ]
                }
            ]
        }]
    });

    function getDateFormat(value) {
        //
        var yyyy = value.getFullYear().toString();
        var m = value.getMonth() + 1;
        var d = value.getDate();
        var mm = m > 9 ? m.toString() : "0" + m.toString();
        var dd = d > 9 ? d.toString() : "0" + d.toString();
        return yyyy + "-" + mm + "-" + dd;
    }

    function clearT31QueryFilter() {
        var f = T31Query.getForm();
        f.findField('T31P9').setValue('');
        f.findField('T31P10').setValue('');
        f.findField('T31P11').setValue('');
        f.findField('T31P12').setValue('');
    }

    var T31Store = viewModel.getStore('MedChkItems');
    function T31Load() {
        T31Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T31Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);
        T31Store.getProxy().setExtraParam("chk_ym", T1LastRec.data.CHK_YM);
        T31Store.getProxy().setExtraParam("chk_period", T1LastRec.data.CHK_PERIOD);
        T31Store.getProxy().setExtraParam("chk_type", T1LastRec.data.CHK_TYPE);

        T31Store.getProxy().setExtraParam("seq1", T31Query.getForm().findField('T31P9').getValue());
        T31Store.getProxy().setExtraParam("seq2", T31Query.getForm().findField('T31P10').getValue());
        T31Store.getProxy().setExtraParam("mmcode1", T31Query.getForm().findField('T31P11').getValue());
        T31Store.getProxy().setExtraParam("mmcode2", T31Query.getForm().findField('T31P12').getValue());
        T31Store.getProxy().setExtraParam('chkpredateNullonly', Ext.getCmp('chkpredateNullOnly').getValue() == true ? 'Y' : 'N');

        T31Tool.moveFirst();
    }
    var T33Store = viewModel.getStore('PickUsers');
    function T33Load() {

        T33Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);

        T33Tool.moveFirst();
    }
    var T31Tool = Ext.create('Ext.PagingToolbar', {
        store: T31Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '產生盤點單',
                id: 'btnMedCreateSheet',
                name: 'btnMedCreateSheet',
                handler: function () {

                    //
                    var datas = T31Grid.getStore().data.items;
                    for (var i = 0; i < datas.length; i++) {
                        if (datas[i].data.CHK_PRE_DATE == null) {
                            Ext.Msg.alert('提醒', '請先設定所有項目之預計盤點日期');
                            return;
                        }
                    }

                    T23Load();
                    medPickUserWindow.show();
                }
            }]
    });
    var T33Tool = Ext.create('Ext.PagingToolbar', {
        store: T23Store,
        border: false,
        plain: true,
        //displayInfo: true,
        buttons: [
            {
                text: '確定',
                id: 'btnMedSetPickUser',
                name: 'btnMedSetPickUser',
                handler: function () {
                    var selection = T33Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇盤點人員');
                        return;
                    }

                    var users = [];
                    for (var i = 0; i < selection.length; i++) {
                        users.push(selection[i].data);
                    }

                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            medCreateSheet(users);
                        }
                    }
                    );
                }
            }]
    });
    var T31Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T31Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        height: windowHeight - 90,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T31Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T31Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                text: "",
                dataIndex: 'SEQ',
                width: 40,
                                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "預計盤點日期",
                dataIndex: 'CHK_PRE_DATE',
                width: 100,
            },
            //{
            //    text: "電腦量",
            //    dataIndex: 'INV_QTY',
            //    //width: 60,
            //    align: 'right',
            //    style: 'text-align:left',
            //    width: 60,
            //},
            //{
            //    text: "盤點量",
            //    dataIndex: 'CHK_QTY',
            //    width: 60,
            //    align: 'right',
            //    style: 'text-align:left'
            //},
            //{
            //    text: "儲位 - 量",
            //    dataIndex: 'STORE_LOC_NAME',
            //    width: 120,
            //},
            //{
            //    text: "盤點人員",
            //    dataIndex: 'CHK_UID_NAME',
            //    width: 150,
            //},
            //{
            //    text: "狀態",
            //    dataIndex: 'STATUS',
            //    width: 90,
            //}
        ]
    });
    var T33Grid = Ext.create('Ext.grid.Panel', {
        store: T33Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T33Tool]
            }
        ],
        columns: [
            //{
            //    text: "員工代碼",
            //    dataIndex: 'WH_CHKUID',
            //    width: 120
            //},
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    var medDetailWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        id: 'medDetailWindow',
        modal: true,
        items: [
            {
                xtype: 'container',
                layout: 'fit',
                items: [
                    //T21Query,
                    {
                        xtype: 'panel',
                        itemId: 't21Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '已選定盤點清單',
                        border: false,
                        height: '100%',
                        //minHeight: windowHeight - 90,
                        items: [T31Grid]
                    },
                ],
            }
        ],

        //items: [T21Grid, T22Grid],
        width: "900px",
        height: windowHeight,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點明細管理",
        //listeners: {
        //    close: function (panel, eOpts) {
        //        myMask.hide();
        //    }
        //}
        buttons: [{
            text: '關閉',
            handler: function () {
                medDetailWindow.hide();
                windowNewOpen = true;
                T1Load();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                medDetailWindow.center();
            }
        }
    });
    medDetailWindow.hide();

    var medPickUserWindow = Ext.create('Ext.window.Window', {
        id: 'medPickUserWindow',
        renderTo: Ext.getBody(),
        items: [
            T33Grid
        ],
        modal: true,
        //items: [T21Grid, T22Grid],
        width: "400px",
        height: 400,
        resizable: false,
        draggable: false,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點人員挑選",
        buttons: [{
            text: '關閉',
            handler: function () {
                medPickUserWindow.hide();
            }
        }],
        listeners: {
            show: function (self, eOpts) {
                medPickUserWindow.setY(0);
            }
        }
    });
    medPickUserWindow.hide();

    function medCreateSheet(users) {
        var myMask = new Ext.LoadMask(Ext.getCmp('medPickUserWindow'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/MedCreateSheet',
            method: reqVal_p,
            params: {
                users: Ext.util.JSON.encode(users),
                chk_no: T1LastRec.data.CHK_NO,
                chk_ym: T1LastRec.data.CHK_YM
            },
            success: function (response) {
                //
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();

                    windowNewOpen = true;
                    T1LastRec.data.CHK_STATUS = '1';

                    T1Load(false);

                    medPickUserWindow.hide();
                    medDetailWindow.hide();

                    msglabel("產生盤點單成功");


                } else {
                    Ext.Msg.alert('提醒', data.msg);
                    myMask.hide();
                }
            },
            failure: function (response, options) {
                Ext.Msg.alert('失敗', '發生例外錯誤');
                myMask.hide();
            }
        });
    }

    // #endregion

    //#region 多筆列印
    var T25Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    border: false,
                    layout: 'hbox',
                    width: 250,
                    items: [
                        {
                            xtype: 'radiogroup',
                            name: 'multiPrintType',
                            fieldLabel: '列印格式',
                            items: [
                                { boxLabel: '依院內碼', width: 70, name: 'multiPrintType', inputValue: 'mmcode', checked: true },
                                { boxLabel: '依單號', width: 70, name: 'multiPrintType', inputValue: 'chk_no' },
                            ]
                        },
                    ]
                },
            ]
        }
        ]
    });

    var multiPrintWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        items: [
            T25Form
        ],
        resizable: false,
        draggable: false,
        closable: false,
        title: "多筆列印",
        buttons: [
            {
                text: '確定',
                handler: function () {

                    var selection = T1Grid.getSelection();

                    var chk_nos = "";
                    for (var i = 0; i < selection.length; i++) {
                        if (i == 0) {
                            chk_nos = selection[i].data.CHK_NO;
                        } else {
                            chk_nos = chk_nos + "," + selection[i].data.CHK_NO;
                        }
                    }

                    if (T25Form.getForm().findField('multiPrintType').getValue()['multiPrintType'] == 'mmcode') {
                        if (selection[0].data.CHK_WH_KIND == 'E' || selection[0].data.CHK_WH_KIND == 'C') {
                            showReport(reportMultiMmcodeUrl, '', chk_nos);
                        } else {
                            showReport(reportMultiMmcodeWardUrl, '', chk_nos);
                        }
                    } else {
                        showReport(reportMultiChknoUrl, "", chk_nos);
                    }

                    multiPrintWindow.hide();
                }
            },
            {
                text: '取消',
                handler: function () {
                    multiPrintWindow.hide();
                }
            }
        ]
    });
    multiPrintWindow.hide();
    //#endregion

    //#region 病房科室衛材盤點單新增項目 addItemWindow
    var T41Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            labelAlign: 'right',
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP41',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'textfield',
                            fieldLabel: '院內碼',
                            name: 'MMCODE',
                            maxLength: 13,
                        },
                        {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                var f = T41Form.getForm();

                                T41Load();
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                            }
                        },
                    ]
                },
            ]
        }]
    });

    var T41Store = viewModel.getStore('AddItemList');
    function T41Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())

        T41Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T41Store.getProxy().setExtraParam("wh_no", T1LastRec.data.CHK_WH_NO);
        T41Store.getProxy().setExtraParam("chk_wh_grade", T1LastRec.data.CHK_WH_GRADE);
        T41Store.getProxy().setExtraParam("chk_wh_kind", T1LastRec.data.CHK_WH_KIND);
        T41Store.getProxy().setExtraParam("chk_type", T1LastRec.data.CHK_TYPE);
        T41Store.getProxy().setExtraParam("mmcode", T41Form.getForm().findField('MMCODE').getValue());
        T41Tool.moveFirst();
    }

    var T41Tool = Ext.create('Ext.PagingToolbar', {
        store: T41Store,
        border: false,
        plain: true,
        displayInfo: true,
        buttons: [
            {
                text: '加入',
                id: 'T41addItem',
                name: 'T41addItem',
                handler: function () {
                    var selection = T41Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請先選擇項目');
                        return;
                    }

                    Ext.MessageBox.confirm('加入項目', '是否確定新增？', function (btn, text) {
                        if (btn === 'yes') {
                            var list = [];

                            for (var i = 0; i < selection.length; i++) {
                                list.push({
                                    CHK_NO: T1LastRec.data.CHK_NO,
                                    WH_NO: T1LastRec.data.CHK_WH_NO,
                                    MMCODE: selection[i].data.MMCODE
                                });
                            }

                            addItem(list);
                        }
                    });
                }
            }
        ]
    });

    var T41Grid = Ext.create('Ext.grid.Panel', {
        store: T41Store,
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                items: [T41Form]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T41Tool]
            }
        ],
        columns: [
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 120
            },
            {
                text: "中文名稱",
                dataIndex: 'MMNAME_C',
                width: 150
            },
            {
                text: "英文名稱",
                dataIndex: 'MMNAME_E',
                width: 150
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "電腦量",
                dataIndex: 'INV_QTY',
                width: 60,
                align: 'right',
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    function addItem(list) {

        Ext.Ajax.request({
            url: '/api/CE0002/AddItems',
            method: reqVal_p,
            params: { list: Ext.util.JSON.encode(list) },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    msglabel('新增項目成功');
                    T41Load();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    var addItemWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'addItemWindow',
        items: [T41Grid],
        resizable: false,
        draggable: false,
        closable: false,
        title: "新增項目",
        buttons: [
            {
                text: '取消',
                handler: function () {

                    windowNewOpen = true;
                    T21Load(true);
                    addItemWindow.hide();
                }
            }
        ]
    });
    addItemWindow.hide();

    //#endregion

    //#region 修改盤點人員 btnMedChangeUid
    var T5Store = viewModel.getStore('CurrentUids');
    function T5Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())

        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }

        T5Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T5Store.getProxy().setExtraParam("is_ward", isWard);

        T5Tool.moveFirst();
    }

    var T5Tool = Ext.create('Ext.PagingToolbar', {
        store: T5Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    //#region CheckboxModel
    Ext.define('overrides.selection.CheckboxModel', {
        override: 'Ext.selection.CheckboxModel',

        getHeaderConfig: function () {
            var config = this.callParent();

            if (Ext.isFunction(this.selectable)) {
                config.selectable = this.selectable;
                config.renderer = function (value, metaData, record, rowIndex, colIndex, store, view) {
                    if (this.selectable(record)) {
                        record.selectable = false;
                        return '';
                    }
                    record.selectable = true;
                    return this.defaultRenderer();
                };
                this.on('beforeselect', function (rowModel, record, index, eOpts) {
                    return !this.selectable(record);
                }, this);
            }
            return config;
        },

        updateHeaderState: function () {
            // check to see if all records are selected
            var me = this,
                store = me.store,
                storeCount = store.getCount(),
                views = me.views,
                hdSelectStatus = false,
                selectedCount = 0,
                selected, len, i, notSelectableRowsCount = 0;

            if (!store.isBufferedStore && storeCount > 0) {
                hdSelectStatus = true;
                store.each(function (record) {
                    if (!record.selectable) {
                        notSelectableRowsCount++;
                    }
                }, this);
                selected = me.selected;

                for (i = 0, len = selected.getCount(); i < len; ++i) {
                    if (store.indexOfId(selected.getAt(i).id) > -1) {
                        ++selectedCount;
                    }
                }
                hdSelectStatus = storeCount === selectedCount; // + notSelectableRowsCount;
            }

            if (views && views.length) {
                me.column.setHeaderStatus(hdSelectStatus);
            }
        }
    });
    //#endregion

    var T5Grid = Ext.create('Ext.grid.Panel', {
        store: T5Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return record.data.HAS_ENTRY == 'Y';
            }
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T5Tool]
            }
        ],
        columns: [
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                text: "備註",
                dataIndex: 'HAS_ENTRY',
                flex: 1,
                renderer: function (val, meta, record) {
                    if (record.data.HAS_ENTRY == 'Y') {
                        return '人員已輸入盤點量，無法調整';
                    }
                    return '';
                },
            }
        ],
        viewConfig: {
            stripeRows: true,
            listeners: {
                beforerefresh: function (view) {

                    var store = view.getStore();
                    var model = view.getSelectionModel();
                    var s = [];
                    store.queryBy(function (record) {
                        if (record.get('IS_SELECTED') === 'Y') {
                            s.push(record);
                        }
                    });
                    model.select(s);

                },
                //beforeselect: function (grid, record, index, eOpts) {
                //    
                //    if (record.get('HAS_ENTRY') == 'Y') // && record.get('IS_SELECTED') === 'Y') {//replace this with your logic.
                //        {
                //        return false;
                //    } else {
                //        return true;
                //    }
                //}
            },

        }
    });

    function changeMedUid() {
        var selection = T5Grid.getSelection();
        var users = [];

        for (var i = 0; i < selection.length; i++) {
            users.push({
                WH_CHKUID: selection[i].data.WH_CHKUID,
                HAS_ENTRY: selection[i].data.HAS_ENTRY,
            });
        }
        var store = T5Grid.getStore().data.items;
        for (var i = 0; i < store.length; i++) {
            if (store[i].data.HAS_ENTRY == 'Y') {
                users.push({
                    WH_CHKUID: store[i].data.WH_CHKUID,
                    HAS_ENTRY: store[i].data.HAS_ENTRY,
                });
            }
        }
        if (users.length == 0) {
            Ext.Msg.alert('提醒', '請至少選擇一名人員');
            return;
        }

        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }
        var myMask = new Ext.LoadMask(Ext.getCmp('medChangeUidWindow'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/ChangeUid',
            method: reqVal_p,
            params: {
                users: Ext.util.JSON.encode(users),
                chk_no: T1LastRec.data.CHK_NO,
                chk_ym: T1LastRec.data.CHK_YM,
                is_ward: isWard
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    msglabel('盤點人員修改成功');
                    T1Load();
                    medChangeUidWindow.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var medChangeUidWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'medChangeUidWindow',
        items: [T5Grid],
        resizable: true,
        draggable: true,
        closable: false,
        title: "新增項目",
        width: 400,
        buttons: [
            {
                text: '確定',
                handler: function () {
                    changeMedUid();
                }
            },
            {
                text: '取消',
                handler: function () {
                    medChangeUidWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                medChangeUidWindow.center();
                medChangeUidWindow.setWidth(400);
            }
        }
    });
    medChangeUidWindow.hide();
    //#endregion

    //#region 修改盤點人員 btnWardChangeUid
    var T6Store = viewModel.getStore('CurrentUids');
    function T6Load() {
        //T41Form.getForm().findField('MMCODE').setValue(T41Form.getForm().findField('MMCODE').getValue().toUpperCase())
        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }
        T6Store.getProxy().setExtraParam("chk_no", T1LastRec.data.CHK_NO);
        T6Store.getProxy().setExtraParam("is_ward", isWard);

        T6Tool.moveFirst();
    }

    var T6Tool = Ext.create('Ext.PagingToolbar', {
        store: T6Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    var T6Grid = Ext.create('Ext.grid.Panel', {
        store: T6Store,
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'SIMPLE',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return record.data.HAS_ENTRY == 'Y';
            }
        },
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 400,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T6Tool]
            }
        ],
        columns: [
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                text: "備註",
                dataIndex: 'HAS_ENTRY',
                flex: 1,
                renderer: function (val, meta, record) {
                    if (record.data.HAS_ENTRY == 'Y') {
                        return '人員已輸入盤點量，無法調整';
                    }
                    return '';
                },
            }
        ],
        viewConfig: {
            stripeRows: true,
            listeners: {
                beforerefresh: function (view) {

                    var store = view.getStore();
                    var model = view.getSelectionModel();
                    var s = [];
                    store.queryBy(function (record) {
                        if (record.get('IS_SELECTED') === 'Y') {
                            s.push(record);
                        }
                    });
                    model.select(s);

                },
                //beforeselect: function (grid, record, index, eOpts) {
                //    
                //    if (record.get('HAS_ENTRY') == 'Y') // && record.get('IS_SELECTED') === 'Y') {//replace this with your logic.
                //        {
                //        return false;
                //    } else {
                //        return true;
                //    }
                //}
            },

        },
        viewready: function (grid) {
            var view = grid.view;

            // record the current cellIndex
            grid.mon(view, {
                uievent: function (type, view, cell, recordIndex, cellIndex, e) {
                    grid.cellIndex = cellIndex;
                    grid.recordIndex = recordIndex;
                }
            });

            grid.tip = Ext.create('Ext.tip.ToolTip', {
                target: view.el,
                delegate: '.x-grid-cell',
                trackMouse: true,
                renderTo: Ext.getBody(),
                listeners: {
                    beforeshow: function updateTipBody(tip) {

                        var has_entry = grid.getStore().getAt(grid.recordIndex).get('HAS_ENTRY');
                        if (has_entry == 'N') {
                            return false;
                        } else {
                            tip.update('人員已輸入盤點量，無法移除');
                        }
                    }
                }
            });

        }
    });

    function changeWardUid() {
        var selection = T6Grid.getSelection();
        var users = [];

        for (var i = 0; i < selection.length; i++) {
            users.push({
                WH_CHKUID: selection[i].data.WH_CHKUID,
                HAS_ENTRY: selection[i].data.HAS_ENTRY,
            });
        }
        var store = T6Grid.getStore().data.items;
        for (var i = 0; i < store.length; i++) {
            if (store[i].data.HAS_ENTRY == 'Y') {
                users.push({
                    WH_CHKUID: store[i].data.WH_CHKUID,
                    HAS_ENTRY: store[i].data.HAS_ENTRY,
                });
            }
        }
        if (users.length == 0) {
            Ext.Msg.alert('提醒', '請至少選擇一名人員');
            return;
        }

        var isWard = "Y";
        if (T1LastRec.data.CHK_WH_GRADE == '2' && T1LastRec.data.CHK_WH_KIND == '0') {
            isWard = "N";
        }
        var myMask = new Ext.LoadMask(Ext.getCmp('wardChangeUidWindow'), { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/ChangeUid',
            method: reqVal_p,
            params: {
                users: Ext.util.JSON.encode(users),
                chk_no: T1LastRec.data.CHK_NO,
                chk_ym: T1LastRec.data.CHK_YM,
                is_ward: isWard
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    myMask.hide();
                    msglabel('盤點人員修改成功');
                    T1Load();
                    wardChangeUidWindow.hide();
                }
            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var wardChangeUidWindow = Ext.create('Ext.window.Window', {
        renderTo: Ext.getBody(),
        modal: true,
        id: 'wardChangeUidWindow',
        items: [T6Grid],
        resizable: true,
        closable: false,
        title: "新增項目",
        width: 400,
        buttons: [
            {
                text: '確定',
                handler: function () {
                    changeWardUid();
                }
            },
            {
                text: '取消',
                handler: function () {
                    wardChangeUidWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                wardChangeUidWindow.center();
                wardChangeUidWindow.setWidth(400);
            }
        }
    });
    wardChangeUidWindow.hide();
    //#endregion

    //#region 一鍵產生盤點單 
    var T7Tool = Ext.create('Ext.PagingToolbar', {
        store: T23Store,
        border: false,
        plain: true,
        displayInfo: true,
    });
    var T7Grid = Ext.create('Ext.grid.Panel', {
        store: T23Store,
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        selType: 'checkboxmodel',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        height: 380,
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T7Tool]
            }
        ],
        columns: [
            {
                text: "中文姓名",
                dataIndex: 'WH_CHKUID_NAME',
                width: 120
            },
            {
                header: "",
                flex: 1
            }

        ],
    });

    var oneClickPickUserWindow = Ext.create('Ext.window.Window', {
        id: 'oneClickPickUserWindow',
        renderTo: Ext.getBody(),
        items: [T7Grid],
        modal: true,
        width: "400px",
        height: 400,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "盤點人員挑選",
        buttons: [
            {
                text: '確定',
                id: 'btnOneClickCreate',
                name: 'btnOneClickCreate',
                handler: function () {
                    var selection = T7Grid.getSelection();

                    if (selection.length == 0) {
                        Ext.Msg.alert('提醒', '請選擇盤點人員');
                        return;
                    }

                    var user = [];
                    for (var i = 0; i < selection.length; i++) {
                        user.push(selection[i].data);
                    }

                    Ext.MessageBox.confirm('產生盤點單', '確定產生盤點單?', function (btn, text) {
                        if (btn === 'yes') {
                            //createSheet(T21Grid.getStore().data.items, selection, T1LastRec.data.CHK_WH_KIND);
                            oneClickCreateSheet(user);
                        }
                    }
                    );
                }
            }
            , {
                text: '關閉',
                handler: function () {
                    oneClickPickUserWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                oneClickPickUserWindow.center();
                oneClickPickUserWindow.setWidth(400);
            }
        }
    });
    oneClickPickUserWindow.hide();

    var oneClickCreateSheet = function (users) {

        var myMask = new Ext.LoadMask(Ext.getCmp('oneClickPickUserWindow'), { msg: '處理中...' });
        myMask.show();
        var myMask1 = new Ext.LoadMask(Ext.getCmp('eastform'), { msg: '處理中...' });
        myMask1.show();

        Ext.Ajax.request({
            url: '/api/CE0002/CreateSheetOneClick',
            method: reqVal_p,
            params: {
                users: Ext.util.JSON.encode(users),
                master: Ext.util.JSON.encode(temp_mast)
            },
            success: function (response) {

                var data = Ext.decode(response.responseText);
                if (data.success == false) {
                    myMask.hide();
                    myMask1.hide();
                    Ext.Msg.alert('失敗', data.msg);
                    return;
                }

                myMask.hide();
                myMask1.hide();
                T1Query.getForm().findField('P0').setValue(temp_mast.CHK_WH_NO);
                T1Load(false);
                Ext.getCmp('eastform').collapse();
                T1Cleanup();
                oneClickPickUserWindow.hide();
                temp_mast = {};

            },
            failure: function (response, options) {
                myMask.hide();
                // Ext.Msg.alert('失敗', '發生例外錯誤');
            }
        });
    }
    //#endregion

    //#region 2021-10-20 針對開單或盤中的品項新增項目
    function checkNeedDetailAdd(wh_no) {
        Ext.getCmp('addNotExists').hide();
        Ext.Ajax.request({
            url: '/api/CE0002/CheckNeedDetailAdd',
            method: reqVal_p,
            params: {
                wh_no: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    Ext.getCmp('addNotExists').show();
                } else {
                    Ext.getCmp('addNotExists').hide();
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function addNotExists(wh_no) {
        myMask.show();
        Ext.Ajax.request({
            url: '/api/CE0002/AddDetailNotExists',
            method: reqVal_p,
            params: {
                wh_no: wh_no
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                myMask.hide();
                T8Store.removeAll();
                var results = data.etts;
                if (results.length > 0) {
                    for (var i = 0; i < results.length; i++) {
                        T8Store.add(results[i]);
                    }
                }

                addDetailWindow.show();

            },
            failure: function (response, options) {
                myMask.hide();
            }
        });
    }

    var T8Store = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'CHK_TYPE', 'CHK_TYPE_NAME', 'CHK_LEVEL', 'CHK_STATUS', 'CHK_STATUS_NAME',
            'MMCODE_COUNT', 'MMCODE_STRING', 'RESULT']
    });
    var T8Grid = Ext.create('Ext.grid.Panel', {
        store: T8Store,
        id: 'T8Grid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                text: "盤點類別",
                dataIndex: 'CHK_TYPE',
                width: 80,
                renderer: function (val, meta, record) {
                    var chk_type = record.data.CHK_TYPE;
                    if (chk_type == '0') {
                        return '非庫備';
                    }
                    if (chk_type == '1') {
                        return '庫備';
                    }
                    if (chk_type == '3') {
                        return '小額採購';
                    }
                    return '';
                },
            },
            {
                text: "盤點單號",
                dataIndex: 'CHK_NO',
                width: 120
            },
            {
                text: "盤點階段",
                dataIndex: 'CHK_LEVEL',
                width: 80,
                renderer: function (val, meta, record) {
                    var temp = record.data.CHK_LEVEL;
                    if (temp == '') {
                        return '';
                    }
                    if (temp == '1') {
                        return '初盤';
                    }
                    if (temp == '2') {
                        return '複盤';
                    }
                    if (temp == '3') {
                        return '三盤';
                    }
                    return '';
                },
            },
            {
                text: "盤點單狀態",
                dataIndex: 'CHK_STATUS_NAME',
                width: 90
            },
            {
                text: "新增院內碼數量",
                dataIndex: 'MMCODE_COUNT',
                width: 120
            },
            {
                text: "結果",
                dataIndex: 'RESULT',
                flex: 1
            }
        ],
    }); 
    var addDetailWindow = Ext.create('Ext.window.Window', {
        id: 'addDetailWindow',
        renderTo: Ext.getBody(),
        items: [T8Grid],
        modal: true,
        width: "800px",
        height: 200,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "新增開單後異動品項",
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    addDetailWindow.hide();
                    checkNeedDetailAdd(T1Query.getForm().findField('P0').getValue());
                    T1Load();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                addDetailWindow.center();
                addDetailWindow.setWidth(800);
            }
        }
    });
    //#endregion

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
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
                id: 'eastform',
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
                //items: [T1Query, T1Form]
                items: [T1Form]
            }
        ]
    });

    Ext.on('resize', function () {
        windowWidth = $(window).width();
        windowHeight = $(window).height();
        detailWindow.setHeight(windowHeight);
        medDetailWindow.setHeight(windowHeight);
        T21Grid.setHeight(windowHeight / 2 - 62);
        T22Grid.setHeight(windowHeight / 2 - 60);
        T31Grid.setHeight(windowHeight - 90);

        addItemWindow.setHeight(windowHeight);
        addItemWindow.setWidth(windowWidth);

        detailWindow.center();
        medDetailWindow.center();
        addItemWindow.center();
        pickUserWindow.center();
        oneClickPickUserWindow.center();
        medChangeUidWindow.center();
        wardChangeUidWindow.center();

        if (detailWindow.hidden == false) {
            if (Number(T1LastRec.data.CHK_STATUS) > 0) {
                T21Grid.setHeight(windowHeight - 90);
                T22Grid.setHeight(0);
            }
        }
    });

    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
    myMask.hide();
    changeChkPeriod('D');
    Ext.getCmp('btnDelete').disable();
});